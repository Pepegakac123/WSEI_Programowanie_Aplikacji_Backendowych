using AppCore.Repositories;
using AppCore.Services;
using AppCore;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Security;
using WebApi.Middleware;
using WebApi.Services;

namespace WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddAppCoreModule(builder.Configuration);
        builder.Services.AddControllers(); 
        builder.Services.AddParkingEfModule(builder.Configuration);

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();    
        builder.Services.AddProblemDetails();
        
        // mechanizm dostępu do HttpContext
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddJwt(builder.Configuration);
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            using (var scope = app.Services.CreateScope())
            {
                var seeders = scope.ServiceProvider
                    .GetServices<IDataSeeder>()
                    .OrderBy(s => s.Order);

                foreach (var seeder in seeders)
                {
                    await seeder.SeedAsync();
                }
            }
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
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
