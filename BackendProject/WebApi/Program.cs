using AppCore.Repositories;
using Infrastructure.Repositories;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IVehicleRepository, InMemoryVehicleRepository>();
        builder.Services.AddScoped<IParkingSessionRepository, InMemoryParkingSessionRepository>();
        builder.Services.AddScoped<IParkingGateRepository, InMemoryParkingGateRepository>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapGet("/api/vehicles/{licensePlate}", async (IVehicleRepository repository, string licensePlate) =>
            {
                var vehicle = await repository.FindByLicensePlateAsync(licensePlate);
                return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
            })
            .WithName("GetVehicleByLicensePlate");

        app.Run();
    }
}
