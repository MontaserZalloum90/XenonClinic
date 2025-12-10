using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for tenant authentication and signup operations
/// </summary>
public interface ITenantAuthService
{
    /// <summary>
    /// Register a new tenant with trial period
    /// </summary>
    /// <param name="request">Signup details</param>
    /// <returns>Signup result with token and tenant info</returns>
    Task<Result<TenantSignupResponse>> SignupAsync(TenantSignupRequest request);

    /// <summary>
    /// Authenticate a tenant admin and return a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="clientIpAddress">Client IP for audit logging</param>
    /// <returns>Authentication result with token and tenant info</returns>
    Task<Result<TenantLoginResponse>> LoginAsync(TenantLoginRequest request, string? clientIpAddress = null);

    /// <summary>
    /// Get the current tenant context for an authenticated user
    /// </summary>
    /// <param name="tenantId">The tenant ID from JWT claims</param>
    /// <returns>Tenant context with license and subscription info</returns>
    Task<Result<TenantContextDto>> GetCurrentTenantAsync(Guid tenantId);

    /// <summary>
    /// Check if a slug is available for a new tenant
    /// </summary>
    /// <param name="slug">The slug to check</param>
    /// <returns>True if available</returns>
    Task<bool> IsSlugAvailableAsync(string slug);
}
