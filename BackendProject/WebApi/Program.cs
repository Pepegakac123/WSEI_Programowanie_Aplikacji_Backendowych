using AppCore.Repositories;
using AppCore.Services;
using AppCore;
using Infrastructure;
using WebApi.Middleware;
namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddAppCoreModule(builder.Configuration);
        builder.Services.AddControllers(); 
        builder.Services.AddParkingEfModule(builder.Configuration);

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();    
        builder.Services.AddProblemDetails();
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
        app.UseExceptionHandler();
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
