using AppCore.Models;
using Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class ParkingDataSeeder(ParkingDbContext context, ILogger<ParkingDataSeeder> logger) : IDataSeeder
{
    public int Order => 2; 

    public async Task SeedAsync()
    {
        await SeedSettingsAsync();
        await SeedTariffsAsync();
        await SeedGatesAsync();
    }

    private async Task SeedSettingsAsync()
    {
        if (await context.ParkingSettings.AnyAsync()) return;

        context.ParkingSettings.Add(new ParkingSettings
        {
            Id = Guid.NewGuid(),
            TotalSpaces = 50
        });
        
        await context.SaveChangesAsync();
        logger.LogInformation("Zainicjowano ustawienia parkingu (50 miejsc).");
    }

    private async Task SeedTariffsAsync()
    {
        if (await context.ParkingTariff.AnyAsync()) return;

        context.ParkingTariff.Add(new ParkingTariff
        {
            Id = Guid.NewGuid(),
            Name = "Standardowa Taryfa PAB26",
            FreeParkingDuration = TimeSpan.FromMinutes(15),
            HourlyRate = 5.0m,
            DailyMaxRate = 50.0m,
            IsActive = true
        });
        
        await context.SaveChangesAsync();
        logger.LogInformation("Zainicjowano domyślną taryfę parkingową.");
    }

    private async Task SeedGatesAsync()
    {
        if (await context.Set<ParkingGate>().AnyAsync()) return; 

        context.Set<ParkingGate>().AddRange(
            new ParkingGate 
            { 
                Id = Guid.NewGuid(), 
                Name = "Wjazd 1", 
                Type = GateType.Entry
            },
            new ParkingGate 
            { 
                Id = Guid.NewGuid(), 
                Name = "Wyjazd 1",
                Type = GateType.Exit
            }
        );
        
        await context.SaveChangesAsync();
        logger.LogInformation("Zainicjowano bramki parkingowe (Wjazd 1, Wyjazd 1).");
    }
}