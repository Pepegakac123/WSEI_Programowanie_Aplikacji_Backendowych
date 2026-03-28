using AppCore.Dto;

namespace AppCore.Models;

public class ParkingGate : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public GateType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsOperational { get; set; }

    public static implicit operator ParkingGateDto(ParkingGate entity) => new(
        entity.Id,
        entity.Name,
        entity.Type.ToString(),
        entity.Location,
        entity.IsOperational
    );
    public static implicit operator ParkingGate(CreateGateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        Type = Enum.Parse<GateType>(dto.Type, ignoreCase: true),
        Location = dto.Location,
        IsOperational = false
    };
}