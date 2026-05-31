using AppCore.Dto;
using AppCore.Exceptions;
using AppCore.Mapper;
using AppCore.Models;
using AppCore.Services;
using AutoMapper;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTest.Services;

public class ParkingGateServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ParkingDbContext _context;
    private readonly IParkingGateService _service;

    public ParkingGateServiceTests()
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
        services.AddAutoMapper(cfg => {
            cfg.AddMaps(typeof(GateMappingProfile).Assembly);
        });

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

        _service = new ParkingGateService(unitOfWork, mapper);
    }

    [Fact]
    public async Task Add_ShouldCreateNewGate()
    {
        // Arrange
        var dto = new CreateGateDto("Nowa Brama", "Entry", "Południe");

        // Act
        var result = await _service.Add(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nowa Brama", result.Name);
        Assert.Equal("Entry", result.Type);
        
        var gateInDb = await _context.ParkingGate.FindAsync(result.Id);
        Assert.NotNull(gateInDb);
        Assert.Equal("Nowa Brama", gateInDb.Name);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnGate()
    {
        // Arrange
        var gate = new ParkingGate { Id = Guid.NewGuid(), Name = "Brama 1", Type = GateType.Entry, Location = "Północ", IsOperational = true };
        _context.ParkingGate.Add(gate);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetById(gate.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Brama 1", result.Name);
    }

    [Fact]
    public async Task Update_ShouldModifyExistingGate()
    {
        // Arrange
        var gate = new ParkingGate { Id = Guid.NewGuid(), Name = "Stara Nazwa", Type = GateType.Entry, Location = "Północ", IsOperational = true };
        _context.ParkingGate.Add(gate);
        await _context.SaveChangesAsync();
        var updateDto = new UpdateGateDto("Nowa Nazwa", "Exit");

        // Act
        var result = await _service.Update(gate.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nowa Nazwa", result.Name);
        Assert.Equal("Exit", result.Type);
    }

    [Fact]
    public async Task ChangeOperationalStatus_ShouldUpdateStatus()
    {
        // Arrange
        var gate = new ParkingGate { Id = Guid.NewGuid(), Name = "Brama", Type = GateType.Entry, IsOperational = true };
        _context.ParkingGate.Add(gate);
        await _context.SaveChangesAsync();

        // Act
        await _service.ChangeOperationalStatus(gate.Id, false);

        // Assert
        var gateInDb = await _context.ParkingGate.FindAsync(gate.Id);
        Assert.False(gateInDb!.IsOperational);
    }

    [Fact]
    public async Task AddCapture_WhenGateExists_ShouldAddCapture()
    {
        // Arrange
        var gate = new ParkingGate { Id = Guid.NewGuid(), Name = "Wjazd 1", Type = GateType.Entry, IsOperational = true };
        _context.ParkingGate.Add(gate);
        await _context.SaveChangesAsync();
        var captureDto = new CreateCameraCaptureDto("ABC-123", "Audi", "Black", "/path/to/img.jpg", "Entry");

        // Act
        var result = await _service.AddCapture(gate.Id, captureDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Wjazd 1", result.GateName);
        Assert.Equal("ABC-123", result.LicensePlate);
        
        var captureInDb = await _context.CameraCapture.FirstOrDefaultAsync(c => c.LicensePlate == "ABC-123");
        Assert.NotNull(captureInDb);
    }

    [Fact]
    public async Task AddCapture_WhenGateNotFound_ShouldThrowException()
    {
        // Arrange
        var captureDto = new CreateCameraCaptureDto("ABC-123", "Audi", "Black", "/path/to/img.jpg", "Entry");

        // Act & Assert
        await Assert.ThrowsAsync<GateNotFoundException>(() => _service.AddCapture(Guid.NewGuid(), captureDto));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
