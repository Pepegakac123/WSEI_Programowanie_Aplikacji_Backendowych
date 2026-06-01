using AppCore.Authorization;
using AppCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


public record ParkingEntryAndExitRequest(string GateName, string LicensePlate);

[ApiController]
[Route("api/[controller]")]
public class SessionsController(IParkingSessionService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("entry")]
    public async Task<IActionResult> HandleEntry([FromBody] ParkingEntryAndExitRequest request)
    {
        var result = await service.HandleEntry(request.GateName, request.LicensePlate);
        return Ok(result);
    }
    [AllowAnonymous]
    [HttpPost("exit")]
    public async Task<IActionResult> HandleExit([FromBody] ParkingEntryAndExitRequest request)
    {
        var result = await service.HandleExit(request.GateName, request.LicensePlate);
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(AppPolicies.AdminOnly))]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var result = await service.GetActiveSessionsAsync();
        return Ok(result);
    }
}