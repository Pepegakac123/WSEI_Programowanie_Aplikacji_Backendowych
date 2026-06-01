using AppCore.Dto;
using AppCore.Exceptions;
using AppCore.Models;
using AppCore.Repositories;
using AutoMapper;

namespace AppCore.Services;

public class ParkingGateService(IParkingUnitOfWork unit, IMapper mapper, ICurrentUserService currentUserService) : IParkingGateService
{
    public async Task<ParkingGateDto?> GetById(Guid id)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        
        if (entity is null)
        {
            return null;
        }
        return mapper.Map<ParkingGateDto>(entity); 
    }
    public async Task<PagedResult<ParkingGateDto>> GetPaged(int page, int pageSize)
    {
        var result = await unit.Gates.FindPagedAsync(page, pageSize);
        var dtoItems = result.Items.Select(e => mapper.Map<ParkingGateDto>(e)).ToList();
        
        return new PagedResult<ParkingGateDto>(dtoItems, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<ParkingGateDto?> GetByName(string name)
    {
        var entity = await unit.Gates.FindByNameAsync(name);
        
        if (entity is null)
        {
            return null;
        }

        return mapper.Map<ParkingGateDto>(entity);
    }

    public async Task<ParkingGateDto> Add(CreateGateDto newGate)
    {
        var entity = mapper.Map<ParkingGate>(newGate);
        await unit.Gates.AddAsync(entity);
        await unit.SaveChangesAsync();
        return mapper.Map<ParkingGateDto>(entity);
    }

    public async Task<ParkingGateDto?> Update(Guid id, UpdateGateDto updateGate)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity.CreatedByUserId);

        entity.Name = updateGate.Name;
        entity.Type = Enum.Parse<GateType>(updateGate.Type, ignoreCase: true);

        await unit.Gates.UpdateAsync(entity);
        await unit.SaveChangesAsync();

        return mapper.Map<ParkingGateDto>(entity);
    }

    public async Task ChangeOperationalStatus(Guid id, bool isOperational)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        
        if (entity is not null)
        {
            EnsureOwnershipOrAdmin(entity.CreatedByUserId);
            entity.IsOperational = isOperational;
            
            await unit.Gates.UpdateAsync(entity);
            await unit.SaveChangesAsync();
        }
    }

    public async Task<CameraCaptureDto> AddCapture(Guid gateId, CreateCameraCaptureDto dto)
    {
        var gate = await unit.Gates.FindByIdAsync(gateId);
        
        if (gate == null)
        {
            throw new GateNotFoundException($"Gate with id={gateId} not found!");
        }

        var capture = mapper.Map<CameraCapture>(dto);
        capture.GateName = gate.Name;

        await unit.CameraCaptures.AddAsync(capture);
        gate.CameraCaptures.Add(capture);
    
        await unit.SaveChangesAsync();

        return mapper.Map<CameraCaptureDto>(capture);
    }

    public async Task<IEnumerable<CameraCaptureDto>> GetCaptures(Guid gateId)
    {
        var entity = await unit.Gates.FindByIdAsync(gateId);
        if (entity is null)
        {
            throw new GateNotFoundException($"Gate with id={gateId} not found!");
        }
        return mapper.Map<IEnumerable<CameraCaptureDto>>(entity.CameraCaptures);
    }

    public async Task DeleteCapture(Guid gateId, Guid captureId)
    {
        var entity = await unit.Gates.FindByIdAsync(gateId);
        if (entity is null)
        {
            throw new GateNotFoundException($"Gate with id={gateId} not found!");
        }

        EnsureOwnershipOrAdmin(entity.CreatedByUserId);

        var capture = entity.CameraCaptures.FirstOrDefault(c => c.Id == captureId);
        if (capture is not null)
        {
            entity.CameraCaptures.Remove(capture);
            await unit.CameraCaptures.RemoveByIdAsync(captureId);
            await unit.SaveChangesAsync();
        }
    }

    private void EnsureOwnershipOrAdmin(string? createdByUserId)
    {
        if (currentUserService.IsAdmin) return;
        
        if (createdByUserId != currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("Nie masz uprawnień do wykonania tej operacji na tym zasobie.");
        }
    }
}
