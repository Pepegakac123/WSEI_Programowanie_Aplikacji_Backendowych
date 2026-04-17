using AppCore.Dto;

namespace AppCore.Models;

public class Vehicle : EntityBase
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public static implicit operator VehicleDto(Vehicle entity) => new(
        entity.Id,
        entity.LicensePlate,
        entity.Brand,
        entity.Color
    );
}
