using AppCore.Authorization.Dto;

namespace AppCore.Authorization;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dti);
    Task RevokeTokenAsync(string refreshToken);
}