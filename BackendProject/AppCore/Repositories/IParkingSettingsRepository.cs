using AppCore.Models;

namespace AppCore.Repositories;

public interface IParkingSettingsRepository : IGenericRepositoryAsync<ParkingSettings>
{
    Task<int> GetTotalSpacesAsync(); 
}