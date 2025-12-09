using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xenon.Platform.Domain.Entities;

namespace Xenon.Platform.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateTenantToken(TenantAdmin admin, Tenant tenant);
    string GeneratePlatformAdminToken(PlatformAdmin admin);
    ClaimsPrincipal? ValidateToken(string token, string audience);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _tenantAudience;
    private readonly string _adminAudience;
    private readonly int _tenantTokenExpiryHours;
    private readonly int _adminTokenExpiryHours;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "xenon-platform";
        _tenantAudience = configuration["Jwt:TenantAudience"] ?? "xenon-tenant";
        _adminAudience = configuration["Jwt:AdminAudience"] ?? "xenon-admin";
        _tenantTokenExpiryHours = int.Parse(configuration["Jwt:TenantTokenExpiryHours"] ?? "24");
        _adminTokenExpiryHours = int.Parse(configuration["Jwt:AdminTokenExpiryHours"] ?? "8");
    }

    public string GenerateTenantToken(TenantAdmin admin, Tenant tenant)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, admin.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", tenant.Id.ToString()),
            new("tenant_slug", tenant.Slug),
            new("tenant_name", tenant.Name),
            new("role", admin.Role),
            new("first_name", admin.FirstName),
            new("last_name", admin.LastName),
            new("realm", "tenant")
        };

        return GenerateToken(claims, _tenantAudience, TimeSpan.FromHours(_tenantTokenExpiryHours));
    }

    public string GeneratePlatformAdminToken(PlatformAdmin admin)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, admin.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", admin.Role),
            new("first_name", admin.FirstName),
            new("last_name", admin.LastName),
            new("realm", "platform-admin")
        };

        // Add permissions as claims
        foreach (var permission in admin.GetPermissions())
        {
            claims.Add(new Claim("permission", permission));
        }

        return GenerateToken(claims, _adminAudience, TimeSpan.FromHours(_adminTokenExpiryHours));
    }

    public ClaimsPrincipal? ValidateToken(string token, string audience)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    private string GenerateToken(List<Claim> claims, string audience, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
