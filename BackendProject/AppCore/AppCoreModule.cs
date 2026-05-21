using AppCore.Validators;
using AppCore.Mapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCore;

public static class AppCoreModule
{
    public static IServiceCollection AddAppCoreModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Rejestracja walidatorów
        services.AddValidatorsFromAssemblyContaining<ParkingGateValidator>();
        
        // dodanie automatycznej walidacji
        services.AddFluentValidationAutoValidation();

        // Rejestracja AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(AppCoreModule)));
        
        return services;
    }
}
