using System;
using AppCore.Dto;

namespace AppCore.Models;

public class CameraCapture : EntityBase
{
    public string GateName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string DetectedBrand { get; set; } = string.Empty;
    public string DetectedColor { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public CaptureType CaptureType { get; set; }
    public DateTime CapturedAt { get; set; }
}
