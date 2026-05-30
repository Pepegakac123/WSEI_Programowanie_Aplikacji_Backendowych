using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.UnitOfWork;

public class EfParkingUnitOfWork(IParkingGateRepository gatesRepository, IVehicleRepository vehicleRepository, ICameraCaptureRepository cameraCaptureRepository, IParkingTariffRepository parkingTariffRepository, IParkingSessionRepository parkingSessionRepository, IParkingSettingsRepository parkingSettingsRepository , ParkingDbContext context) : IParkingUnitOfWork
{
    public IParkingGateRepository Gates => gatesRepository;
    public IVehicleRepository Vehicles => vehicleRepository;
    public IParkingSessionRepository Sessions => parkingSessionRepository;
    public IParkingTariffRepository Tariffs => parkingTariffRepository;
    public ICameraCaptureRepository CameraCaptures => cameraCaptureRepository;
    public IParkingSettingsRepository ParkingSettings => parkingSettingsRepository;
    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();

    public Task BeginTransactionAsync() => context.Database.BeginTransactionAsync();
    public Task CommitTransactionAsync() => context.Database.CommitTransactionAsync();
    public Task RollbackTransactionAsync() => context.Database.RollbackTransactionAsync();
}