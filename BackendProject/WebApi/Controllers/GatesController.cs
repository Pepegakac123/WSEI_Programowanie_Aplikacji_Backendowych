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
    public async Task<IActionResult> GetById(Guid id)
    {
        var gate = await service.GetById(id);
        if (gate == null) return NotFound();
        return Ok(gate);
    }

    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var gate = await service.GetByName(name);
        if (gate == null) return NotFound();
        return Ok(gate);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateGateDto createGateDto)
    {
        var gate = await service.Add(createGateDto);
        return CreatedAtAction(nameof(GetById), new { id = gate.Id }, gate);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] bool isOperational)
    {
        await service.ChangeOperationalStatus(id, isOperational);
        return NoContent();
    }
}
