using System;

namespace AppCore.Dto;

public enum GateAction
{
    Open,
    KeepClosed,
    RequirePayment
};

public record ParkingEntryResultDto(
    Guid? SessionId,
    VehicleDto? Vehicle,
    string GateName,
    DateTime? EntryTime,
    string Message,
    GateAction GateAction
);

public record ParkingExitResultDto(
    Guid? SessionId,
    VehicleDto? Vehicle,
    string GateName,
    DateTime EntryTime,
    DateTime ExitTime,
    TimeSpan Duration,
    TimeSpan FreeParkingDuration,
    decimal Fee,
    bool WasCharged,
    string Message,
    GateAction GateAction
);

public record ActiveParkingSessionDto(
    Guid SessionId,
    VehicleDto Vehicle,
    string GateName,
    DateTime EntryTime,
    TimeSpan CurrentDuration
);

public record ParkingSessionHistoryDto(
    Guid SessionId,
    VehicleDto Vehicle,
    string GateName,
    DateTime EntryTime,
    DateTime? ExitTime,
    TimeSpan? Duration,
    decimal? Fee,
    bool IsActive
);
