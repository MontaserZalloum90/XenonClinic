using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for platform admin tenant management operations
/// </summary>
public interface ITenantManagementService
{
    /// <summary>
    /// Get paginated list of tenants with optional filtering
    /// </summary>
    Task<PagedResult<TenantListItemDto>> GetTenantsAsync(TenantListQuery query);

    /// <summary>
    /// Get full details of a specific tenant
    /// </summary>
    Task<Result<TenantFullDetailsDto>> GetTenantDetailsAsync(Guid tenantId);

    /// <summary>
    /// Suspend a tenant account
    /// </summary>
    Task<Result> SuspendTenantAsync(Guid tenantId, SuspendTenantRequest request);

    /// <summary>
    /// Activate a suspended tenant account
    /// </summary>
    Task<Result> ActivateTenantAsync(Guid tenantId);

    /// <summary>
    /// Extend a tenant's trial period
    /// </summary>
    Task<Result<ExtendTrialResponse>> ExtendTrialAsync(Guid tenantId, ExtendTrialRequest request);

    /// <summary>
    /// Get tenant usage metrics
    /// </summary>
    Task<Result<TenantUsageDto>> GetTenantUsageAsync(Guid tenantId, TenantUsageQuery query);
}
