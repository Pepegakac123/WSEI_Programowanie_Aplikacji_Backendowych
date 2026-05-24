using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Infrastructure.Security;

public class JwtSettings(IConfiguration configuration)
{
    private static readonly string Section = "Jwt"; 
    public string? Issuer => configuration.GetSection(Section).GetSection("Issuer").Value ?? throw new InvalidOperationException("Issuer is not set.");
    public string? Audience => configuration.GetSection(Section).GetSection("Audience").Value ?? throw new InvalidOperationException("Audience is not set.");
    public string? Secret => configuration.GetSection(Section).GetSection("SecretKey").Value ?? throw new InvalidOperationException("Secret key is not set.");
    public int ExpirationInMinutes => configuration.GetSection(Section).GetSection("ExpiryInMinutes").Get<int>();
    public int RefreshTokenDays => configuration.GetSection(Section).GetSection("RefreshTokenDays").Get<int>();
    public SymmetricSecurityKey GetSymmetricKey() =>
        new(Encoding.UTF8.GetBytes(Secret ?? throw new InvalidOperationException("Secret key is not set.")));
}