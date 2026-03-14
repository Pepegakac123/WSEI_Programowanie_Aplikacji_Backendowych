using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Memory;

public class InMemoryParkingSessionRepository : MemoryGenericRepository<ParkingSession>, IParkingSessionRepository
{
    public Task<ParkingSession?> FindByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values.FirstOrDefault(ps => ps.Vehicle.LicensePlate == licensePlate && ps.IsActive);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<ParkingSession>> FindActiveSessionsAsync()
    {
        var result = _data.Values.Where(ps => ps.IsActive).ToList();
        return Task.FromResult<IEnumerable<ParkingSession>>(result);
    }

    public Task<IEnumerable<ParkingSession>> FindHistoryByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values.Where(ps => ps.Vehicle.LicensePlate == licensePlate).ToList();
        return Task.FromResult<IEnumerable<ParkingSession>>(result);
    }
}
