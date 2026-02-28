using AppCore.ValueObjects;

namespace AppCore.Models;

public class Car
{
    public int Id { get; set;}
    public required PlateNumber PlateNumber { get; set; }
    public DateTime Entry { get; set; }
    public DateTime? Exit { get; set; }
}