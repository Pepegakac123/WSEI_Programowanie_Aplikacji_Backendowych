using AppCore.Repositories;
using Infrastructure.Memory;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<ICarRepository, MemoryCarRepository>();

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

       

        app.MapGet("/api/cars/{number}", async (ICarRepository repository, string number,HttpContext context) =>
            {
                return await repository.FindByPlateNumber(number);
            })
            .WithName("GetWeatherForecast");

        app.Run();
    }
}