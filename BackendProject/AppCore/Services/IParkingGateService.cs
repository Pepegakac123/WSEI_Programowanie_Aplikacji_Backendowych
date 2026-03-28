using AppCore.Dto;

namespace AppCore.Services;

public interface IParkingGateService
{
    Task<ParkingGateDto?> GetById(Guid id);
    Task<PagedResult<ParkingGateDto>> GetPaged(int page, int pageSize);
    Task<ParkingGateDto?> GetByName(string name);
    Task<ParkingGateDto> Add(CreateGateDto newGate);
    Task ChangeOperationalStatus(Guid id, bool isOperational);
}