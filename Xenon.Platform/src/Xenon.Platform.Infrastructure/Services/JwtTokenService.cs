using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateTenantToken(TenantAdmin admin, Tenant tenant);
    string GeneratePlatformAdminToken(PlatformAdmin admin);
    string GenerateTwoFactorToken(Guid userId, string userType, string email);
    ClaimsPrincipal? ValidateToken(string token, string audience);
    /// <summary>
    /// BUG FIX: Validates the JWT token and checks if the user has invalidated all tokens
    /// after this token was issued. This is the recommended method to use for secure validation.
    /// </summary>
    Task<ClaimsPrincipal?> ValidateTokenWithInvalidationCheckAsync(string token, string audience);
    ClaimsPrincipal? ValidateTwoFactorToken(string token);
    DateTime GetTokenExpiry(string token);
    DateTime? GetTokenIssuedAt(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly PlatformDbContext _context;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _tenantAudience;
    private readonly string _adminAudience;
    private readonly int _tenantTokenExpiryHours;
    private readonly int _adminTokenExpiryHours;
    private readonly string _twoFactorAudience;
    private readonly int _twoFactorTokenExpiryMinutes;

    public JwtTokenService(IConfiguration configuration, PlatformDbContext context)
    {
        _configuration = configuration;
        _context = context;
        _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "xenon-platform";
        _tenantAudience = configuration["Jwt:TenantAudience"] ?? "xenon-tenant";
        _adminAudience = configuration["Jwt:AdminAudience"] ?? "xenon-admin";
        _tenantTokenExpiryHours = int.Parse(configuration["Jwt:TenantTokenExpiryHours"] ?? "24");
        _adminTokenExpiryHours = int.Parse(configuration["Jwt:AdminTokenExpiryHours"] ?? "8");
        _twoFactorAudience = configuration["Jwt:TwoFactorAudience"] ?? "xenon-2fa";
        _twoFactorTokenExpiryMinutes = int.Parse(configuration["Jwt:TwoFactorTokenExpiryMinutes"] ?? "5");
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

    public string GenerateTwoFactorToken(Guid userId, string userType, string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("user_type", userType),
            new("purpose", "2fa-verification")
        };

        return GenerateToken(claims, _twoFactorAudience, TimeSpan.FromMinutes(_twoFactorTokenExpiryMinutes));
    }

    public ClaimsPrincipal? ValidateTwoFactorToken(string token)
    {
        var principal = ValidateToken(token, _twoFactorAudience);

        if (principal == null)
            return null;

        // Verify it's a 2FA token
        var purposeClaim = principal.FindFirst("purpose")?.Value;
        if (purposeClaim != "2fa-verification")
            return null;

        return principal;
    }

    public DateTime GetTokenExpiry(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// BUG FIX: Gets the issued at time from the JWT token.
    /// Used to compare against TokenInvalidatedAt for security validation.
    /// </summary>
    public DateTime? GetTokenIssuedAt(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.IssuedAt;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// BUG FIX: Validates the JWT token and checks if the user has invalidated all tokens
    /// after this token was issued. This prevents invalidated tokens from being used.
    /// </summary>
    public async Task<ClaimsPrincipal?> ValidateTokenWithInvalidationCheckAsync(string token, string audience)
    {
        // First, perform standard JWT validation
        var principal = ValidateToken(token, audience);
        if (principal == null)
        {
            return null;
        }

        // Get the user ID and realm from claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
        var realmClaim = principal.FindFirst("realm");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        // Get the token issued at time
        var issuedAt = GetTokenIssuedAt(token);
        if (!issuedAt.HasValue)
        {
            return null;
        }

        // Check if user has invalidated tokens after this token was issued
        DateTime? tokenInvalidatedAt = null;
        var realm = realmClaim?.Value;

        if (realm == "platform-admin")
        {
            tokenInvalidatedAt = await _context.PlatformAdmins
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => a.TokenInvalidatedAt)
                .FirstOrDefaultAsync();
        }
        else if (realm == "tenant")
        {
            tokenInvalidatedAt = await _context.TenantAdmins
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => a.TokenInvalidatedAt)
                .FirstOrDefaultAsync();
        }

        // If tokens were invalidated after this token was issued, reject it
        if (tokenInvalidatedAt.HasValue && issuedAt.Value < tokenInvalidatedAt.Value)
        {
            return null;
        }

        return principal;
    }

    private string GenerateToken(List<Claim> claims, string audience, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Add issued at claim if not present
        if (!claims.Any(c => c.Type == JwtRegisteredClaimNames.Iat))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64));
        }

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
