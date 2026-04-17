using System.Threading.Tasks;

namespace AppCore.Repositories;

public interface IParkingUnitOfWork
{
    IVehicleRepository Vehicles { get; }
    IParkingGateRepository Gates { get; }
    IParkingSessionRepository Sessions { get; }
    IParkingTariffRepository Tariffs { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}