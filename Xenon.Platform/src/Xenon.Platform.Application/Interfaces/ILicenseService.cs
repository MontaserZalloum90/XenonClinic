using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for tenant license validation and usage tracking
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Get license limits and current usage for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>License information with guardrails</returns>
    Task<Result<LicenseInfoDto>> GetLicenseAsync(Guid tenantId);

    /// <summary>
    /// Update tenant usage counters
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="request">Usage update</param>
    /// <returns>Updated usage information</returns>
    Task<Result<UsageUpdateResponse>> UpdateUsageAsync(Guid tenantId, UsageUpdateRequest request);
}
