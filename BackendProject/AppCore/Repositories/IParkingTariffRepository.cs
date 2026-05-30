using AppCore.Models;

namespace AppCore.Repositories;

public interface IParkingTariffRepository : IGenericRepositoryAsync<ParkingTariff>
{
    Task<ParkingTariff?> GetActiveTariffAsync();
}
