using System;

using AppCore.Models;

namespace AppCore.Dto;

public record ParkingTariffDto(
    Guid Id,
    string Name,
    TimeSpan FreeParkingDuration,
    decimal HourlyRate,
    decimal DailyMaxRate,
    bool IsActive
);

public record CreateTariffDto(
    string Name,
    int FreeMinutes,
    decimal HourlyRate,
    decimal DailyMaxRate
)
{
    public ParkingTariff ToEntity()
    {
        return new ParkingTariff()
        {
            Name = Name,
            FreeParkingDuration = TimeSpan.FromMinutes(FreeMinutes),
            HourlyRate = HourlyRate,
            DailyMaxRate = DailyMaxRate,
            IsActive = false,
        };
    }
}
