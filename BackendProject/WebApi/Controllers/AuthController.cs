using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AppCore.Authorization;
using AppCore.Authorization.Dto;
using AppCore.Exceptions;
using System.Security.Claims;
using System.Linq;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Logowanie — zwraca access token i refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await authService.LoginAsync(dto);
            return Ok(response); 
        }
        catch (InvalidCredentialsException e)
        {
            return Unauthorized(new { message = e.Message });
        }
        catch (UserStatusException e)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = e.Message });
        }
    }

    /// <summary>Odświeżenie access tokenu.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous] 
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(dto);
            return Ok(response); 
        }
        catch (TokenException e)
        {
            return Unauthorized(new { message = e.Message });
        }
    }

    /// <summary>Wylogowanie — unieważnia refresh token.</summary>
    [HttpPost("revoke")]
    [Authorize] 
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        try
        {
            await authService.RevokeTokenAsync(refreshToken);
            return NoContent();
        }
        catch (TokenException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    /// <summary>Dane zalogowanego użytkownika pobrane bezpośrednio z tokenu.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(SystemUserDto), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var user = new SystemUserDto
        {
            Id         = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Email      = User.FindFirstValue(ClaimTypes.Email)!,
            FirstName  = User.FindFirstValue(ClaimTypes.GivenName)!,
            LastName   = User.FindFirstValue(ClaimTypes.Surname)!,
            Department = User.FindFirstValue("department")!,
            Roles      = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        };

        return Ok(user);
    }
}