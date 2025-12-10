using System.Security.Claims;

namespace Xenon.Platform.Api.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to simplify claim extraction
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the tenant ID from the user's claims
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>The tenant ID if found and valid, null otherwise</returns>
    public static Guid? GetTenantId(this ClaimsPrincipal user)
    {
        var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            return null;
        }

        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }

    /// <summary>
    /// Gets the tenant ID from the user's claims, throwing if not found
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>The tenant ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when tenant ID is not found or invalid</exception>
    public static Guid GetRequiredTenantId(this ClaimsPrincipal user)
    {
        var tenantId = user.GetTenantId();
        if (!tenantId.HasValue)
        {
            throw new UnauthorizedAccessException("Tenant ID not found in claims");
        }
        return tenantId.Value;
    }

    /// <summary>
    /// Gets the user ID from the user's claims
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>The user ID if found and valid, null otherwise</returns>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the user email from the user's claims
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>The user email if found, null otherwise</returns>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Gets the realm from the user's claims (tenant or platform-admin)
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>The realm if found, null otherwise</returns>
    public static string? GetRealm(this ClaimsPrincipal user)
    {
        return user.FindFirst("realm")?.Value;
    }

    /// <summary>
    /// Checks if the user is a platform admin
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>True if the user is a platform admin</returns>
    public static bool IsPlatformAdmin(this ClaimsPrincipal user)
    {
        return user.GetRealm() == "platform-admin";
    }

    /// <summary>
    /// Checks if the user is a tenant user
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>True if the user is a tenant user</returns>
    public static bool IsTenantUser(this ClaimsPrincipal user)
    {
        return user.GetRealm() == "tenant";
    }
}
