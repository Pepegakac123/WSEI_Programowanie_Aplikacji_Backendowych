using Microsoft.AspNetCore.Mvc; 
using AppCore.Services;  
using System.Threading.Tasks; 
namespace WebApi.Controllers; 

[ApiController]
[Route("/api/[controller]")]
public class GatesController(IParkingGateService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllGates(int page = 1, int size = 10)
    {
        return Ok(await service.GetPaged(page, size));
    }
}