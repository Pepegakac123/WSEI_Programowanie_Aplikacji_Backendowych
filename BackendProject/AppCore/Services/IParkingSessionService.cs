using AppCore.Dto;

namespace AppCore.Services;

public interface IParkingSessionService
{
    Task<ParkingEntryResultDto> HandleEntry(string gateName, string licensePlate);
    Task<ParkingExitResultDto> HandleExit(string gateName, string licensePlate);
    Task<IEnumerable<ActiveParkingSessionDto>> GetActiveSessionsAsync();
}