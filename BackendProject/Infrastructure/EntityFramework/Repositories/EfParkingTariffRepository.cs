using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingTariffRepository(ParkingDbContext context) : EfGenericRepository<ParkingTariff>(context.ParkingTariff), IParkingTariffRepository
{
    public async Task<ParkingTariff?> GetActiveTariffAsync()
    {
        return await context.ParkingTariff.FirstOrDefaultAsync(t => t.IsActive);
    }
}