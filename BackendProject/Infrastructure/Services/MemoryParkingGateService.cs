using AppCore.Dto;
using AppCore.Models;
using AppCore.Services;
using AppCore.Repositories;
using AutoMapper;

namespace Infrastructure.Services;

public class MemoryParkingGateService(IParkingUnitOfWork unit, IMapper mapper) : IParkingGateService
{
    private readonly IMapper _mapper = mapper;

    public async Task<ParkingGateDto?> GetById(Guid id)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        
        if (entity is null)
        {
            return null;
        }
        return _mapper.Map<ParkingGateDto>(entity); 
    }
    public async Task<PagedResult<ParkingGateDto>> GetPaged(int page, int pageSize)
    {
        var result = await unit.Gates.FindPagedAsync(page, pageSize);
        var dtoItems = result.Items.Select(e => _mapper.Map<ParkingGateDto>(e)).ToList();
        
        return new PagedResult<ParkingGateDto>(dtoItems, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<ParkingGateDto?> GetByName(string name)
    {
        var entity = await unit.Gates.FindByNameAsync(name);
        
        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<ParkingGateDto>(entity);
    }

    public async Task<ParkingGateDto> Add(CreateGateDto newGate)
    {
        ParkingGate entity = newGate; 
        await unit.Gates.AddAsync(entity);
        await unit.SaveChangesAsync();
        return _mapper.Map<ParkingGateDto>(entity);
    }

    public async Task<ParkingGateDto?> Update(Guid id, UpdateGateDto updateGate)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        if (entity is null)
        {
            return null;
        }

        entity.Name = updateGate.Name;
        entity.Type = Enum.Parse<GateType>(updateGate.Type, ignoreCase: true);

        await unit.Gates.UpdateAsync(entity);
        await unit.SaveChangesAsync();

        return _mapper.Map<ParkingGateDto>(entity);
    }

    public async Task ChangeOperationalStatus(Guid id, bool isOperational)
    {
        var entity = await unit.Gates.FindByIdAsync(id);
        
        if (entity is not null)
        {
            entity.IsOperational = isOperational;
            
            await unit.Gates.UpdateAsync(entity);
            await unit.SaveChangesAsync();
        }
    }
}
