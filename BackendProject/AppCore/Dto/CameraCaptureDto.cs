namespace AppCore.Dto;

public record CameraCaptureDto(
    string LicensePlate,
    string Brand,
    string Color,
    string GateName,
    string? ImagePath = null
);

public record CreateCameraCaptureDto(
    string LicensePlate,
    string Brand,
    string Color,
    string? ImagePath,
    string CaptureType // Wjazd lub Wyjazd
);