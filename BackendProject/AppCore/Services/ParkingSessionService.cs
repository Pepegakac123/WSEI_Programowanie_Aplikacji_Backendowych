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
        var gate = await unit.Gates.FindByNameAsync(gateName);
        if (gate == null)
        {
            throw new GateNotFoundException("The gate does not exist.");
        }

        var vehicle = await unit.Vehicles.FindByLicensePlateAsync(licensePlate);
        if (vehicle == null)
        {
            var (brand, color) = SmartCameraSimulator.RecognizeDetails();
            vehicle = new Vehicle{ Id = Guid.NewGuid(), LicensePlate = licensePlate,  Brand = brand, Color = color };
            await unit.Vehicles.AddAsync(vehicle);
        }
        var vehicleDto = mapper.Map<VehicleDto>(vehicle);
        var totalSpaces = await unit.ParkingSettings.GetTotalSpacesAsync();
        var parkingSpacesTaken = await unit.Sessions.FindActiveSessionsAsync();
        if (totalSpaces - parkingSpacesTaken.Count() == 0)
        {
            return new ParkingEntryResultDto(Guid.Empty,vehicleDto,gateName,null,ParkingMessages.NoAvailableSpaces,GateAction.KeepClosed);
        }

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            GateName = gate.Name,
            EntryTime = DateTime.Now,
            ExitTime = null,
            ParkingFee = null,
            IsActive = true
        };
        await unit.Sessions.AddAsync(session);
        await unit.SaveChangesAsync();
        return new ParkingEntryResultDto(session.Id,vehicleDto,gateName,DateTime.Now,ParkingMessages.Welcome,GateAction.Open);
    
    }

    public async Task<ParkingExitResultDto> HandleExit(string gateName, string licensePlate)
    {
        var gate = await unit.Gates.FindByNameAsync(gateName);
        if (gate == null)
        {
            throw new GateNotFoundException("The gate does not exist.");
        }
        
        var sessions = await unit.Sessions.FindActiveSessionsAsync();
        var currentSession = sessions?.FirstOrDefault(s => s.IsActive && s.Vehicle.LicensePlate == licensePlate);
        if (currentSession == null)
        {

            return new ParkingExitResultDto(
                Guid.Empty, null, gateName, DateTime.MinValue, DateTime.Now, 
                TimeSpan.Zero, TimeSpan.Zero, 0, false, 
                ParkingMessages.ForbbidenEntry, 
                GateAction.KeepClosed
            );
        }
        var vehicle = currentSession.Vehicle;
        var vehicleDto = mapper.Map<VehicleDto>(vehicle);

        var totalDuration = DateTime.Now - currentSession.EntryTime;
        
        var activeTariff = await unit.Tariffs.GetActiveTariffAsync();
        if (activeTariff == null)
        {
            activeTariff = new ParkingTariff
            {
                Name = "Domyślna Taryfa Awaryjna",
                FreeParkingDuration = TimeSpan.FromHours(1),
                HourlyRate = 5,
                DailyMaxRate = 15,
                IsActive = true
            };
        }
        // SCENARIUSZ A: Zmieścił się w darmowym czasie
        if (totalDuration <= activeTariff.FreeParkingDuration)
        {
            currentSession.IsActive = false;
            currentSession.ExitTime = DateTime.Now;
            await unit.SaveChangesAsync();

            return new ParkingExitResultDto(currentSession.Id,vehicleDto,gate.Name,currentSession.EntryTime,DateTime.Now,totalDuration,activeTariff.FreeParkingDuration,0,false,ParkingMessages.Goodbye,GateAction.Open);
        }

// SCENARIUSZ B: Zapłacił w parkometrze i wyjeżdża w ciągu 10 minut
        if (currentSession is { ParkingFee: not null, PaymentTime: not null })
        {
            var timeSincePayment = DateTime.Now - currentSession.PaymentTime.Value;
            if (timeSincePayment.TotalMinutes <= 10)
            {
                currentSession.IsActive = false;
                currentSession.ExitTime = DateTime.Now;
                await unit.SaveChangesAsync();
        
                return new ParkingExitResultDto(currentSession.Id,
                    vehicleDto,
                    gate.Name,
                    currentSession.EntryTime,
                    DateTime.Now,
                    totalDuration,
                    activeTariff.FreeParkingDuration,
                    currentSession.ParkingFee.Value,
                    true,
                    ParkingMessages.GoodbyeWithPaymentConfirmationMsg(currentSession.ParkingFee.Value),
                    GateAction.Open);
            }
        }

// SCENARIUSZ C: Przekroczył czas lub nie opłacił (żądanie zapłaty)
        var payableTime = totalDuration-activeTariff.FreeParkingDuration;
        decimal calculatedFee = (decimal)Math.Ceiling(payableTime.TotalHours) * activeTariff.HourlyRate;
        decimal fee = calculatedFee < activeTariff.DailyMaxRate  ? calculatedFee : activeTariff.DailyMaxRate;
        return new ParkingExitResultDto(currentSession.Id,
            vehicleDto,
            gate.Name,
            currentSession.EntryTime,
            DateTime.Now,
            totalDuration,
            activeTariff.FreeParkingDuration,
            fee,
            false,
            ParkingMessages.FeeMsg(fee),
            GateAction.RequirePayment);
    }

    public async Task<IEnumerable<ActiveParkingSessionDto>> GetActiveSessionsAsync()
    {
        var entity = await unit.Sessions.FindActiveSessionsAsync();
        return mapper.Map<IEnumerable<ActiveParkingSessionDto>>(entity);
    }
        
}
