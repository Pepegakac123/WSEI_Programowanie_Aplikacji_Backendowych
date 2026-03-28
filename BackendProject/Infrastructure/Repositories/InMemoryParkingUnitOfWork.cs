using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class MemoryParkingUnitOfWork(
    IVehicleRepository vehicles,
    IParkingSessionRepository sessions,
    IParkingGateRepository gates
): IParkingUnitOfWork
{
    public IVehicleRepository Vehicles => vehicles;
    public IParkingGateRepository Gates => gates;
    public IParkingSessionRepository Sessions => sessions;
    
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