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
            Id = Guid.Parse("718873E4-4284-4054-9988-825B81F14674"),
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
            Id = Guid.Parse("F5C1B1E1-9B5A-4B6A-9D4A-8B6A9B5A4B6A"),
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
                Id = Guid.Parse("52f48199-bc0a-477c-afbe-78b59aced549"), 
                Name = "Wjazd 1", 
                Type = GateType.Entry,
                IsOperational = true 
            },
            new ParkingGate 
            { 
                Id = Guid.Parse("1984c840-e04a-464f-8cad-861ef2e7abd1"), 
                Name = "Wyjazd 1",
                Type = GateType.Exit,
                IsOperational = true 
            }
        );
        
        await context.SaveChangesAsync();
        logger.LogInformation("Zainicjowano bramki parkingowe (Wjazd 1, Wyjazd 1).");
    }
}