using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.UnitOfWork;

public class EfParkingUnitOfWork(IParkingGateRepository gatesRepository, ParkingDbContext context) : IParkingUnitOfWork
{
    public IParkingGateRepository Gates => gatesRepository;
    
    // Na razie zostawmy te właściwości rzucające błąd, dopóki nie zrobisz reszty repozytoriów EF:
    public IVehicleRepository Vehicles => throw new NotImplementedException();
    public IParkingSessionRepository Sessions => throw new NotImplementedException();
    public IParkingTariffRepository Tariffs => throw new NotImplementedException();
    public ICameraCaptureRepository CameraCaptures => throw new NotImplementedException();
    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();

    public Task BeginTransactionAsync() => context.Database.BeginTransactionAsync();
    public Task CommitTransactionAsync() => context.Database.CommitTransactionAsync();
    public Task RollbackTransactionAsync() => context.Database.RollbackTransactionAsync();
}