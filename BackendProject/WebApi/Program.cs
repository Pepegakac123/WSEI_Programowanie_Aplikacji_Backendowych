using AppCore.Repositories;
using AppCore.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddControllers(); 
        builder.Services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
        builder.Services.AddSingleton<IParkingSessionRepository, InMemoryParkingSessionRepository>();
        builder.Services.AddSingleton<IParkingGateRepository, InMemoryParkingGateRepository>();
        builder.Services.AddSingleton<IParkingGateService, MemoryParkingGateService>();
        builder.Services.AddSingleton<IParkingUnitOfWork, InMemoryParkingUnitOfWork>();

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
        app.MapControllers();  
        app.MapGet("/api/vehicles/{licensePlate}", async (IVehicleRepository repository, string licensePlate) =>
            {
                var vehicle = await repository.FindByLicensePlateAsync(licensePlate);
                return vehicle is not null ? Results.Ok(vehicle) : Results.NotFound();
            })
            .WithName("GetVehicleByLicensePlate");

        app.Run();
    }
}
