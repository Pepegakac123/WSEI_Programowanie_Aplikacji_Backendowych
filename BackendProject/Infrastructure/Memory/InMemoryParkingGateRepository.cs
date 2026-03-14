using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;

namespace Infrastructure.Memory;

public class InMemoryParkingGateRepository : MemoryGenericRepository<ParkingGate>, IParkingGateRepository
{
    public Task<ParkingGate?> FindByNameAsync(string name)
    {
        var result = _data.Values.FirstOrDefault(g => g.Name == name);
        return Task.FromResult(result);
    }
}
