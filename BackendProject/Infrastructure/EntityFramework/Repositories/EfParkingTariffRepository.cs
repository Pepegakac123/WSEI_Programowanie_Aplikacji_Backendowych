using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingTariffRepository(ParkingDbContext context) : EfGenericRepository<ParkingTariff>(context.ParkingTariff), IParkingTariffRepository
{
    
}