using AppCore.Dto;

namespace AppCore.Services;

public interface IParkingGateService
{
    Task<ParkingGateDto?> GetById(Guid id);
    Task<PagedResult<ParkingGateDto>> GetPaged(int page, int pageSize);
    Task<ParkingGateDto?> GetByName(string name);
    Task<ParkingGateDto> Add(CreateGateDto newGate);
    Task<ParkingGateDto?> Update(Guid id, UpdateGateDto updateGate);
    Task ChangeOperationalStatus(Guid id, bool isOperational);
    Task<CameraCaptureDto> AddCapture(Guid gateId, CreateCameraCaptureDto dto);
    Task<IEnumerable<CameraCaptureDto>> GetCaptures(Guid gateId);
    Task DeleteCapture(Guid gateId, Guid captureId);
}