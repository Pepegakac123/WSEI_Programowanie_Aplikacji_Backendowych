using System.Threading.Tasks;
using AppCore.Models;

namespace AppCore.Repositories;

public interface IParkingGateRepository : IGenericRepositoryAsync<ParkingGate>
{
    Task<ParkingGate?> FindByNameAsync(string name);
}
