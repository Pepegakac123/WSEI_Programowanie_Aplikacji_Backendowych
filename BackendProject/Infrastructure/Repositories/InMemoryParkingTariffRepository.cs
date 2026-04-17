using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Repositories;

public class InMemoryParkingTariffRepository : MemoryGenericRepository<ParkingTariff>, IParkingTariffRepository
{
}
