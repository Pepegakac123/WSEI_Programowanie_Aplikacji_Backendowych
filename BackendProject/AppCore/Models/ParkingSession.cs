using System;

using AppCore.Dto;

namespace AppCore.Models;

public class ParkingSession : EntityBase
{
    public Guid VehicleId { get; set; }
    public required Vehicle Vehicle { get; set; }
    public string GateName { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal? ParkingFee { get; set; }
    public bool IsActive { get; set; }

    public static implicit operator ActiveParkingSessionDto(ParkingSession entity) =>
        new(
            entity.Id,
            (VehicleDto)entity.Vehicle,
            entity.GateName,
            entity.EntryTime,
            DateTime.Now - entity.EntryTime
        );

    public static implicit operator ParkingSessionHistoryDto(ParkingSession entity) =>
        new(
            entity.Id,
            (VehicleDto)entity.Vehicle,
            entity.GateName,
            entity.EntryTime,
            entity.ExitTime,
            entity.ExitTime - entity.EntryTime,
            entity.ParkingFee,
            entity.IsActive
        );
}
