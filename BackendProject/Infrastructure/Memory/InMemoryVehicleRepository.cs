using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Memory;

public class InMemoryVehicleRepository : MemoryGenericRepository<Vehicle>, IVehicleRepository
{
    public Task<Vehicle?> FindByLicensePlateAsync(string licensePlate)
    {
        var result = _data.Values.FirstOrDefault(v => v.LicensePlate == licensePlate);
        return Task.FromResult(result);
    }
}
