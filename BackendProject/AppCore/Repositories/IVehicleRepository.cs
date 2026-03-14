using System.Threading.Tasks;
using AppCore.Models;

namespace AppCore.Repositories;

public interface IVehicleRepository : IGenericRepositoryAsync<Vehicle>
{
    Task<Vehicle?> FindByLicensePlateAsync(string licensePlate);
}
