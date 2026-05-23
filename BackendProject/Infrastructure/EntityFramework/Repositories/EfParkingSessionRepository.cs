using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingSessionRepository(ParkingDbContext context): EfGenericRepository<ParkingSession>(context.ParkingSession), IParkingSessionRepository
{
    public async Task<ParkingSession?> FindByLicensePlateAsync(string licensePlate)
    {
        return await context.ParkingSession
            .Include(p => p.Vehicle)
            .FirstOrDefaultAsync(p => p.Vehicle.LicensePlate == licensePlate && p.ExitTime == null);
    }

    public async Task<IEnumerable<ParkingSession>> FindActiveSessionsAsync()
    {
        return await context.ParkingSession
            .Include(s => s.Vehicle)
            .Where(s => s.ExitTime == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<ParkingSession>> FindHistoryByLicensePlateAsync(string licensePlate)
    {
        return await context.ParkingSession
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle.LicensePlate == licensePlate)
            .ToListAsync();
    }
}