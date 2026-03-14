using System;

namespace AppCore.Dto;

public record VehicleDto(
    Guid Id,
    string LicensePlate,
    string Brand,
    string Color
);
