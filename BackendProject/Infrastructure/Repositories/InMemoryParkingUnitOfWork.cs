using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryParkingUnitOfWork(
    IVehicleRepository vehicles,
    IParkingSessionRepository sessions,
    IParkingGateRepository gates,
    IParkingTariffRepository tariffs ,
    ICameraCaptureRepository cameraCaptures
): IParkingUnitOfWork
{
    public IVehicleRepository Vehicles => vehicles;
    public IParkingGateRepository Gates => gates;
    public IParkingSessionRepository Sessions => sessions;
    public IParkingTariffRepository Tariffs => tariffs;
    public ICameraCaptureRepository CameraCaptures => cameraCaptures;
    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync()
    {
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync()
    {
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync()
    {
        return Task.CompletedTask;
    }
}