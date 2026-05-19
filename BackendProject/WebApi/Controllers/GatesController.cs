using Microsoft.AspNetCore.Mvc; 
using AppCore.Services;  
using AppCore.Dto;
using System;
using System.Threading.Tasks; 

namespace WebApi.Controllers; 

[ApiController]
[Route("/api/[controller]")]
public class GatesController(IParkingGateService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllGates([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        return Ok(await service.GetPaged(page, size));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGate(Guid id)
    {
        var dto = await service.GetById(id);
        if (dto == null)
        {
            return NotFound();
        }
        return Ok(dto);
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var dto = await service.GetByName(name);
        if (dto == null)
        {
            return NotFound();
        }
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGate([FromBody] CreateGateDto dto)
    {
        var result = await service.Add(dto);
        return CreatedAtAction(nameof(GetGate), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateGate(Guid id, [FromBody] UpdateGateDto dto)
    {
        var result = await service.Update(id, dto);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] bool isOperational)
    {
        await service.ChangeOperationalStatus(id, isOperational);
        return NoContent();
    }
    
    [HttpPost("{gateId:guid}/captures")]
    public async Task<IActionResult> AddCameraCapture(
        [FromRoute] Guid gateId,
        [FromBody] CreateCameraCaptureDto dto)
    {
        var capture = await service.AddCapture(gateId, dto);
        return CreatedAtAction(
            nameof(GetCaptures),
            new { gateId },
            capture
        );
    }

    [HttpGet("{gateId:guid}/captures")]
    public async Task<IActionResult> GetCaptures([FromRoute] Guid gateId)
    {
        var captures = await service.GetCaptures(gateId);
        return Ok(captures);
    }

    [HttpDelete("{gateId:guid}/captures/{captureId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCapture([FromRoute] Guid gateId, [FromRoute] Guid captureId)
    {
        await service.DeleteCapture(gateId, captureId);
        return NoContent();
    }
}
