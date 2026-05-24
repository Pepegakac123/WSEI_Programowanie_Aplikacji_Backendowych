using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AppCore.Authorization;
using AppCore.Authorization.Dto;
using AppCore.Exceptions;
using AppCore.Users;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class AuthService(UserManager<AppUser> userManager, ParkingDbContext context, JwtSettings jwtOptions) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email) ??
                   throw new InvalidCredentialsException();
        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw new InvalidCredentialsException();
        }

        if (user.Status != SystemUserStatus.Active)
            throw new UserStatusException("Konto jest nieaktywne");
        if (await userManager.IsLockedOutAsync(user))
            throw new UserStatusException("Konto jest zablokowane.");
        await userManager.ResetAccessFailedCountAsync(user);
        await userManager.UpdateAsync(user);
        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? throw new TokenException("Nieprawidłowy token.");

        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new TokenException("Użytkownik nie istnieje.");

        var refreshToken = await context.RefreshTokens
                               .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.UserId == userId)
                           ?? throw new TokenException("Nieprawidłowy refresh token.");

        if (!refreshToken.IsActive)
            throw new TokenException("Refresh token wygasł lub został odwołany.");

        var newResponse = await GenerateAuthResponseAsync(user);
        refreshToken.Revoke(newResponse.RefreshToken);
        await context.SaveChangesAsync();

        return newResponse;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
                        .FirstOrDefaultAsync(t => t.Token == refreshToken)
                    ?? throw new TokenException("Token nie istnieje.");

        if (!token.IsActive)
            throw new TokenException("Token jest już nieaktywny.");

        token.Revoke();
        await context.SaveChangesAsync();
    }
    
    private async Task<AuthResponseDto> GenerateAuthResponseAsync(AppUser user)
    {
        var roles       = await userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt    = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
            User = new SystemUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Status = user.Status,
                Email = user.Email ?? string.Empty,
                Roles = roles
            }
        };
    }
    private string GenerateAccessToken(AppUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email,          user.Email ?? string.Empty),
            new(ClaimTypes.GivenName,      user.FirstName),
            new(ClaimTypes.Surname,        user.LastName),
            new("status",                  user.Status.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(jwtOptions.GetSymmetricKey(), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer:             jwtOptions.Issuer,
            audience:           jwtOptions.Audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
            token.Revoke();

        var refreshToken = new RefreshToken
        {
            UserId    = userId,
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        };

        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        return refreshToken;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtOptions.Issuer,
            ValidAudience            = jwtOptions.Audience,
            IssuerSigningKey         = jwtOptions.GetSymmetricKey()
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(accessToken, parameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new TokenException("Nieprawidłowy token.");
            }

            return principal;
        }
        catch (Exception)
        {
            throw new TokenException("Nieprawidłowy token.");
        }
    }
}