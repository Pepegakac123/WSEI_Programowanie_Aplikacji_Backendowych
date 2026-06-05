using AppCore.Constants;
using AppCore.Dto;
using AppCore.Exceptions;
using AppCore.Mapper;
using AppCore.Models;
using AppCore.Services;
using AppCore.Validators;
using AutoMapper;
using FluentValidation;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTest.Services;


public class TestCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; } = "test-user-id";
    public bool IsAdmin { get; set; } = false;
}
public class ParkingSessionServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ParkingDbContext _context;
    private readonly IParkingSessionService _service;

    public ParkingSessionServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<ParkingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ParkingDbContext(options, new TestCurrentUserService());
        _context.Database.EnsureCreated();
        
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(VehicleMappingProfile))); 

        var serviceProvider = services.BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        
        var unitOfWork = new EfParkingUnitOfWork(
            new EfParkingGateRepository(_context),
            new EfVehicleRepository(_context),
            new EfCameraCaptureRepository(_context),
            new EfParkingTariffRepository(_context),
            new EfParkingSessionRepository(_context),
            new EfParkingSettingsRepository(_context),
            _context
        );

        var sessionValidator = new ParkingSessionValidator();
        _service = new ParkingSessionService(unitOfWork, mapper, sessionValidator);

        SeedDatabase();
    }
    private void SeedDatabase()
    {
        _context.ParkingSettings.Add(new ParkingSettings { Id = Guid.NewGuid(), TotalSpaces = 50 });
        _context.ParkingGate.Add(new ParkingGate { Id = Guid.NewGuid(), Name = "Wjazd 1", Type = GateType.Entry, IsOperational = true });
        _context.ParkingGate.Add(new ParkingGate { Id = Guid.NewGuid(), Name = "Wyjazd 1", Type = GateType.Exit, IsOperational = true });
        _context.ParkingTariff.Add(new ParkingTariff { Id = Guid.NewGuid(), Name = "Test", FreeParkingDuration = TimeSpan.FromMinutes(15), HourlyRate = 5, DailyMaxRate = 50, IsActive = true });
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task HandleEntry_WhenSpacesAvailable_ShouldOpenGateAndCreateSession()
    {
        string gateName = "Wjazd 1";
        string licensePlate = "TEST-123";
        
        var result = await _service.HandleEntry(gateName, licensePlate);


        Assert.NotNull(result);
        Assert.Equal(GateAction.Open, result.GateAction);
        Assert.Equal(ParkingMessages.Welcome, result.Message);
        Assert.NotEqual(Guid.Empty, result.SessionId);
        
        var sessionInDb = await _context.ParkingSession.FirstOrDefaultAsync(s => s.Vehicle.LicensePlate == licensePlate);
        Assert.NotNull(sessionInDb);
        Assert.True(sessionInDb.IsActive);
    }

    [Fact]
    public async Task HandleEntry_WhenNoSpacesAvailable_ShouldKeepGateClosed()
    {
        var settings = await _context.ParkingSettings.FirstAsync();
        settings.TotalSpaces = 0;
        await _context.SaveChangesAsync();
        
        var result = await _service.HandleEntry("Wjazd 1", "WAW-TEST");
        Assert.NotNull(result);
        Assert.Equal(GateAction.KeepClosed, result.GateAction);
        Assert.Equal(ParkingMessages.NoAvailableSpaces,result.Message);
        Assert.Equal(Guid.Empty, result.SessionId);
    }

    [Fact]
    public async Task HandleExit_WhenFreeTimeExceeded_ShouldRequirePayment()
    {
        var licensePlate = "PAY-123";
        var vehicle = new Vehicle { Id = Guid.NewGuid(), LicensePlate = licensePlate, Brand = "Ford", Color = "Niebieski" };
        await _context.Vehicle.AddAsync(vehicle);

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            GateName = "Wjazd 1",
            EntryTime = DateTime.Now.AddHours(-2), 
            IsActive = true
        };
        await _context.ParkingSession.AddAsync(session);
        await _context.SaveChangesAsync();

        var result = await _service.HandleExit("Wyjazd 1", licensePlate);
        Assert.NotNull(result);
        Assert.Equal(GateAction.RequirePayment, result.GateAction);
        Assert.True(result.Fee > 0);
        var sessionInDb = await _context.ParkingSession.FirstOrDefaultAsync(s => s.Vehicle.LicensePlate == licensePlate);
        Assert.NotNull(sessionInDb);
        Assert.True(sessionInDb.IsActive);
    }
    [Fact]
    public async Task HandleExit_WhenWithinFreeTime_ShouldOpenGate()
    {
        // Arrange - Scenariusz A: Wjazd 5 minut temu (mieści się w darmowych 15 min)
        var licensePlate = "FREE-123";
        var vehicle = new Vehicle { Id = Guid.NewGuid(), LicensePlate = licensePlate, Brand = "Opel", Color = "Czarny" };
        await _context.Vehicle.AddAsync(vehicle);

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(), VehicleId = vehicle.Id, Vehicle = vehicle,
            GateName = "Wjazd 1", EntryTime = DateTime.Now.AddMinutes(-5), IsActive = true
        };
        await _context.ParkingSession.AddAsync(session);
        await _context.SaveChangesAsync();


        var result = await _service.HandleExit("Wyjazd 1", licensePlate);


        Assert.NotNull(result);
        Assert.Equal(GateAction.Open, result.GateAction);
        Assert.Equal(0, result.Fee); 
        
        var sessionInDb = await _context.ParkingSession.FindAsync(session.Id);
        Assert.False(sessionInDb!.IsActive); 
    }

    [Fact]
    public async Task HandleExit_WhenPaidAndWithin10Minutes_ShouldOpenGate()
    {
        // Arrange - Scenariusz B: Wjazd 2h temu, ale zapłacił 5 minut temu
        var licensePlate = "PAID-123";
        var vehicle = new Vehicle { Id = Guid.NewGuid(), LicensePlate = licensePlate, Brand = "BMW", Color = "Biały" };
        await _context.Vehicle.AddAsync(vehicle);

        var session = new ParkingSession
        {
            Id = Guid.NewGuid(), VehicleId = vehicle.Id, Vehicle = vehicle,
            GateName = "Wjazd 1", EntryTime = DateTime.Now.AddHours(-2), IsActive = true,
            ParkingFee = 10m, PaymentTime = DateTime.Now.AddMinutes(-5) // Zapłacono 5 min temu!
        };
        await _context.ParkingSession.AddAsync(session);
        await _context.SaveChangesAsync();

        var result = await _service.HandleExit("Wyjazd 1", licensePlate);
        
        Assert.Equal(GateAction.Open, result.GateAction);
        Assert.True(result.WasCharged);
        
        var sessionInDb = await _context.ParkingSession.FindAsync(session.Id);
        Assert.False(sessionInDb!.IsActive);
    }
    

    [Fact]
    public async Task HandleEntry_WhenGateDoesNotExist_ShouldThrowGateNotFoundException()
    {

        await Assert.ThrowsAsync<GateNotFoundException>(() => 
            _service.HandleEntry("Nieistniejaca Bramka", "TEST-123"));
    }

    [Fact]
    public async Task HandleExit_WhenGateIsWrongType_ShouldThrowInvalidGateOperationException()
    {

        var exception = await Assert.ThrowsAsync<InvalidGateOperationException>(() => 
            _service.HandleExit("Wjazd 1", "TEST-123"));
            
        Assert.Contains("nie obsługuje", exception.Message);
    }
    [Fact]
    public async Task HandleEntry_WhenVehicleAlreadyOnParking_ShouldKeepGateClosed()
    {
        // Arrange
        string gateName = "Wjazd 1";
        string licensePlate = "DOUBLE-ENTRY";
        await _service.HandleEntry(gateName, licensePlate);
        
        // Act
        var result = await _service.HandleEntry(gateName, licensePlate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(GateAction.KeepClosed, result.GateAction);
        Assert.Equal(ParkingMessages.CarAlreadyParked, result.Message);
        
        // Check if camera capture was logged for both attempts
        var captures = await _context.CameraCapture.Where(c => c.LicensePlate == licensePlate).ToListAsync();
        Assert.Equal(2, captures.Count);
        Assert.All(captures, c => Assert.Equal(CaptureType.Entry, c.CaptureType));
    }

    [Fact]
    public async Task HandleEntry_ShouldLogCameraCapture()
    {
        // Arrange
        string gateName = "Wjazd 1";
        string licensePlate = "CAPTURE-123";

        // Act
        await _service.HandleEntry(gateName, licensePlate);

        // Assert
        var capture = await _context.CameraCapture.FirstOrDefaultAsync(c => c.LicensePlate == licensePlate);
        Assert.NotNull(capture);
        Assert.Equal(gateName, capture.GateName);
        Assert.Equal(CaptureType.Entry, capture.CaptureType);
        Assert.NotNull(capture.ImagePath);
    }

    [Fact]
    public async Task HandleExit_ShouldLogCameraCapture()
    {
        // Arrange
        string entryGate = "Wjazd 1";
        string exitGate = "Wyjazd 1";
        string licensePlate = "EXIT-CAPTURE";
        await _service.HandleEntry(entryGate, licensePlate);

        // Act
        await _service.HandleExit(exitGate, licensePlate);

        // Assert
        var capture = await _context.CameraCapture
            .FirstOrDefaultAsync(c => c.LicensePlate == licensePlate && c.CaptureType == CaptureType.Exit);
        Assert.NotNull(capture);
        Assert.Equal(exitGate, capture.GateName);
        Assert.Equal(CaptureType.Exit, capture.CaptureType);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}