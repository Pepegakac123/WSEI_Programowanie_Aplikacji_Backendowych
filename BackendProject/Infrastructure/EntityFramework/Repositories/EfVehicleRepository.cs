using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfVehicleRepository(ParkingDbContext context)
    : EfGenericRepository<Vehicle>(context.Vehicle), IVehicleRepository
{
    public async Task<Vehicle?> FindByLicensePlateAsync(string licensePlate)
    {
        return await context.Vehicle.FirstOrDefaultAsync(l => l.LicensePlate == licensePlate);
    }
}
