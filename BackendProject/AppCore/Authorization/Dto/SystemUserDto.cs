using System;
using System.Collections.Generic;
using AppCore.Users;

namespace AppCore.Authorization.Dto;

public record LoginDto(string Email, string Password);

public record SystemUserDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public SystemUserStatus Status { get; init; }
    public IEnumerable<string> Roles { get; init; } = [];
}

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public SystemUserDto User { get; init; } = null!;
}

public record RefreshTokenDto(string AccessToken, string RefreshToken);