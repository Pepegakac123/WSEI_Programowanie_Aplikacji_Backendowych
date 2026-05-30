using AppCore.Models;
using AppCore.Repositories;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Repositories;

public class EfParkingSettingsRepository(ParkingDbContext  context) : EfGenericRepository<ParkingSettings>(context.ParkingSettings), IParkingSettingsRepository
{
    public async Task<int> GetTotalSpacesAsync()
    {
        var settings = await context.ParkingSettings.FirstOrDefaultAsync();
        return settings != null ? settings.TotalSpaces : 0;
        
    }
}