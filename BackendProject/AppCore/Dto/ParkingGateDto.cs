using System;

namespace AppCore.Dto;

public record ParkingGateDto(
    Guid Id,
    string Name,
    string Type,
    string Location,
    bool IsOperational
);

public record CreateGateDto(
    string Name,
    string Type,
    string Location
);

public record UpdateGateDto(
    string Name,
    string Type
);
