using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingGateRepository(ParkingDbContext context) : EfGenericRepository<ParkingGate>(context.ParkingGate), IParkingGateRepository
{
    public async Task<ParkingGate?> FindByNameAsync(string name)
    {
        return await context.ParkingGate.FirstOrDefaultAsync(g => g.Name == name);
    }
}