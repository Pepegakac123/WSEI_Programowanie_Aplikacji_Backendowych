using AppCore.Dto;

namespace AppCore.Models;

public class Vehicle : EntityBase
{
    public string LicensePlate { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
