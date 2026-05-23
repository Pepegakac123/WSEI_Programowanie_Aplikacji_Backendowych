using AppCore.Dto;

namespace AppCore.Models;

public class ParkingGate : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public GateType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsOperational { get; set; }
    public ICollection<CameraCapture> CameraCaptures { get; set; } = new List<CameraCapture>();
}