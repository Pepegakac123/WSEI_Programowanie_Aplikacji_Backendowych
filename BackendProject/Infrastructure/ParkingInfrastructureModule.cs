using AppCore.Authorization;
using AppCore.Repositories;
using AppCore.Services;
using AppCore.Users;
using Infrastructure.Data;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Infrastructure.EntityFramework.Repositories;
using Infrastructure.EntityFramework.UnitOfWork;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ParkingInfrastructureModule
{
    public static IServiceCollection AddParkingEfModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ParkingDbContext>(options => 
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Rejestracja wszystkich repozytoriów bazodanowych:
        services.AddScoped<IVehicleRepository, EfVehicleRepository>(); 
        services.AddScoped<IParkingGateRepository, EfParkingGateRepository>();
        services.AddScoped<ICameraCaptureRepository, EfCameraCaptureRepository>();
        services.AddScoped<IParkingSessionRepository, EfParkingSessionRepository>();
        services.AddScoped<IParkingTariffRepository, EfParkingTariffRepository>();
        services.AddScoped<IParkingSettingsRepository, EfParkingSettingsRepository>();
        
        // Serwisy i Unit of Work:
        services.AddScoped<IParkingUnitOfWork, EfParkingUnitOfWork>();
        services.AddScoped<IParkingGateService, ParkingGateService>();
        services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<ParkingDbContext>() 
            .AddDefaultTokenProviders();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDataSeeder, IdentityDbSeeder>();
        services.AddScoped<IDataSeeder, ParkingDataSeeder>();
        

        return services;
    }
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    
    {
        var jwtOptions = new JwtSettings(configuration);
        services.AddSingleton(jwtOptions);
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = jwtOptions.GetSymmetricKey(),
                    ClockSkew = TimeSpan.Zero 
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = 401,
                            title = "Unauthorized",
                            detail = "Brak autoryzacji lub niepoprawny token."
                        });
                        await context.Response.WriteAsync(result);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppPolicies.AdminOnly.Name(), policy =>
                policy.RequireRole(UserRole.Administrator.ToString()));
            
            options.AddPolicy(AppPolicies.StaffOnly.Name(), policy =>
                policy.RequireRole(UserRole.Administrator.ToString(), UserRole.GateController.ToString()));
            
            options.AddPolicy(AppPolicies.ActiveUser.Name(), policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("status", SystemUserStatus.Active.ToString()));
            
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}