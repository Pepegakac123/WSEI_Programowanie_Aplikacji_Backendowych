using AppCore.Repositories;
using AppCore.Services;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ParkingInfrastructureModule
{
    public static IServiceCollection AddParkingEfModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParkingDbContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IParkingGateRepository, EfParkingGateRepository>();
        services.AddScoped<IParkingUnitOfWork, EfParkingUnitOfWork>();
        services.AddScoped<IParkingGateService,ParkingGateService>();
        return services;
    }
}