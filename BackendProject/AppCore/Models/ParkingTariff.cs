using System;

using AppCore.Dto;

namespace AppCore.Models;

public class ParkingTariff : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan FreeParkingDuration { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyMaxRate { get; set; }
    public bool IsActive { get; set; }

    public static implicit operator ParkingTariffDto(ParkingTariff entity) =>
        new (
            entity.Id,
            entity.Name,
            entity.FreeParkingDuration,
            entity.HourlyRate,
            entity.DailyMaxRate,
            entity.IsActive
        );
}
