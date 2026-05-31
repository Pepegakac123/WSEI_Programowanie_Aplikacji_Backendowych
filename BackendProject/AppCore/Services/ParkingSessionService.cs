using AppCore.Constants;
using AppCore.Dto;
using AppCore.Exceptions;
using AppCore.Models;
using AppCore.Repositories;
using AppCore.Utils;
using AutoMapper;

namespace AppCore.Services;

public class ParkingSessionService(IParkingUnitOfWork unit, IMapper mapper) : IParkingSessionService
{
    public async Task<ParkingEntryResultDto> HandleEntry(string gateName, string licensePlate)
    {
        var gate = await ValidateAndGetGateAsync(gateName, GateType.Entry);
        
        var vehicle = await unit.Vehicles.FindByLicensePlateAsync(licensePlate);
        if (vehicle == null)
        {
            var (brand, color) = SmartCameraSimulator.RecognizeDetails();
            vehicle = new Vehicle { Id = Guid.NewGuid(), LicensePlate = licensePlate, Brand = brand, Color = color };
            await unit.Vehicles.AddAsync(vehicle);
        }
        var vehicleDto = mapper.Map<VehicleDto>(vehicle);
        
        var activeSessions = await unit.Sessions.FindActiveSessionsAsync();

        if (activeSessions.Any(s => s.Vehicle.LicensePlate == licensePlate))
        {
            await LogCaptureAsync(gate.Name, licensePlate, vehicle.Brand, vehicle.Color, CaptureType.Entry);
            await unit.SaveChangesAsync();
            return new ParkingEntryResultDto(Guid.Empty, vehicleDto, gateName, null, "Pojazd znajduje się już na parkingu.", GateAction.KeepClosed);
        }
        
        var totalSpaces = await unit.ParkingSettings.GetTotalSpacesAsync();
        
        if (totalSpaces - activeSessions.Count() <= 0)
        {
            await LogCaptureAsync(gate.Name, licensePlate, vehicle.Brand, vehicle.Color, CaptureType.Entry);
            await unit.SaveChangesAsync();
            return new ParkingEntryResultDto(Guid.Empty, vehicleDto, gateName, null, ParkingMessages.NoAvailableSpaces, GateAction.KeepClosed);
        }

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            GateName = gate.Name,
            EntryTime = DateTime.Now,
            IsActive = true
        };
        
        await unit.Sessions.AddAsync(session);
        await LogCaptureAsync(gate.Name, licensePlate, vehicle.Brand, vehicle.Color, CaptureType.Entry);
        await unit.SaveChangesAsync();
        
        return new ParkingEntryResultDto(session.Id, vehicleDto, gateName, session.EntryTime, ParkingMessages.Welcome, GateAction.Open);
    }

    public async Task<ParkingExitResultDto> HandleExit(string gateName, string licensePlate)
    {
        var gate = await ValidateAndGetGateAsync(gateName, GateType.Exit);
        
        var sessions = await unit.Sessions.FindActiveSessionsAsync();
        var currentSession = sessions?.FirstOrDefault(s => s.IsActive && s.Vehicle.LicensePlate == licensePlate);
        
        if (currentSession == null)
        {
            await LogCaptureAsync(gate.Name, licensePlate, "Nieznany", "Nieznany", CaptureType.Exit);
            await unit.SaveChangesAsync();
            return new ParkingExitResultDto(Guid.Empty, null, gateName, DateTime.MinValue, DateTime.Now, 
                TimeSpan.Zero, TimeSpan.Zero, 0, false, ParkingMessages.ForbbidenEntry, GateAction.KeepClosed);
        }

        var vehicleDto = mapper.Map<VehicleDto>(currentSession.Vehicle);
        var totalDuration = DateTime.Now - currentSession.EntryTime;
        var activeTariff = await GetActiveTariffOrDefaultAsync();

        await LogCaptureAsync(gate.Name, licensePlate, currentSession.Vehicle.Brand, currentSession.Vehicle.Color, CaptureType.Exit);

        // SCENARIUSZ A: Zmieścił się w darmowym czasie
        if (totalDuration <= activeTariff.FreeParkingDuration)
        {
            await CloseSessionAsync(currentSession);
            return new ParkingExitResultDto(currentSession.Id, vehicleDto, gate.Name, currentSession.EntryTime, 
                currentSession.ExitTime!.Value, totalDuration, activeTariff.FreeParkingDuration, 0, false, 
                ParkingMessages.Goodbye, GateAction.Open);
        }

        // SCENARIUSZ B: Zapłacił w parkometrze i wyjeżdża w ciągu 10 minut
        if (currentSession is { ParkingFee: not null, PaymentTime: not null })
        {
            var timeSincePayment = DateTime.Now - currentSession.PaymentTime.Value;
            if (timeSincePayment.TotalMinutes <= 10)
            {
                await CloseSessionAsync(currentSession);
                return new ParkingExitResultDto(currentSession.Id, vehicleDto, gate.Name, currentSession.EntryTime, 
                    currentSession.ExitTime!.Value, totalDuration, activeTariff.FreeParkingDuration, 
                    currentSession.ParkingFee.Value, true, 
                    ParkingMessages.GoodbyeWithPaymentConfirmationMsg(currentSession.ParkingFee.Value), GateAction.Open);
            }
        }

        // SCENARIUSZ C: Przekroczył czas lub nie opłacił (żądanie zapłaty)
        var payableTime = totalDuration - activeTariff.FreeParkingDuration;
        decimal calculatedFee = (decimal)Math.Ceiling(payableTime.TotalHours) * activeTariff.HourlyRate;
        decimal fee = calculatedFee < activeTariff.DailyMaxRate ? calculatedFee : activeTariff.DailyMaxRate;
        
        await unit.SaveChangesAsync();
        return new ParkingExitResultDto(currentSession.Id, vehicleDto, gate.Name, currentSession.EntryTime, 
            DateTime.Now, totalDuration, activeTariff.FreeParkingDuration, fee, false, 
            ParkingMessages.FeeMsg(fee), GateAction.RequirePayment);
    }

    public async Task<IEnumerable<ActiveParkingSessionDto>> GetActiveSessionsAsync()
    {
        var entity = await unit.Sessions.FindActiveSessionsAsync();
        return mapper.Map<IEnumerable<ActiveParkingSessionDto>>(entity);
    }
    
    private async Task<ParkingGate> ValidateAndGetGateAsync(string gateName, GateType expectedType)
    {
        var gate = await unit.Gates.FindByNameAsync(gateName);
        if (gate == null) throw new GateNotFoundException("The gate does not exist.");
        if (gate.Type != expectedType) throw new InvalidGateOperationException($"Ta bramka nie obsługuje operacji typu: {expectedType}.");
        if (!gate.IsOperational) throw new InvalidGateOperationException("Ta bramka jest obecnie wyłączona z użytku.");
        
        return gate;
    }

    private async Task<ParkingTariff> GetActiveTariffOrDefaultAsync()
    {
        var activeTariff = await unit.Tariffs.GetActiveTariffAsync();
        return activeTariff ?? new ParkingTariff
        {
            Name = "Domyślna Taryfa Awaryjna",
            FreeParkingDuration = TimeSpan.FromHours(1),
            HourlyRate = 5,
            DailyMaxRate = 15,
            IsActive = true
        };
    }

    private async Task CloseSessionAsync(ParkingSession session)
    {
        session.IsActive = false;
        session.ExitTime = DateTime.Now;
        await unit.SaveChangesAsync();
    }

    private async Task LogCaptureAsync(string gateName, string licensePlate, string brand, string color, CaptureType type)
    {
        var capture = new CameraCapture
        {
            Id = Guid.NewGuid(),
            GateName = gateName,
            LicensePlate = licensePlate,
            DetectedBrand = brand,
            DetectedColor = color,
            ImagePath = $"/captures/{type.ToString().ToLower()}/{DateTime.Now:yyyyMMddHHmmss}_{licensePlate}.jpg",
            CaptureType = type,
            CapturedAt = DateTime.Now
        };
        await unit.CameraCaptures.AddAsync(capture);
    }
}