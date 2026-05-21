using AppCore.Repositories;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ParkingMemoryModule
{
    public static IServiceCollection AddParkingMemoryModule(this IServiceCollection services)
    {
        // Rejestrujemy komponenty, które tymczasowo zostają w pamięci RAM
        services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
        services.AddSingleton<IParkingSessionRepository, InMemoryParkingSessionRepository>();
        services.AddSingleton<IParkingTariffRepository, InMemoryParkingTariffRepository>();
        services.AddSingleton<ICameraCaptureRepository, InMemoryCameraCaptureRepository>();
        
        return services;
    }
}