using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for tenant-level operations and context management
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current user's tenant ID
    /// </summary>
    Task<int?> GetCurrentTenantIdAsync();

    /// <summary>
    /// Gets the current tenant entity with settings
    /// </summary>
    Task<Tenant?> GetCurrentTenantAsync();

    /// <summary>
    /// Gets the current tenant's settings
    /// </summary>
    Task<TenantSettings?> GetCurrentTenantSettingsAsync();

    /// <summary>
    /// Checks if the current user has access to a specific tenant
    /// </summary>
    Task<bool> HasAccessToTenantAsync(int tenantId);

    /// <summary>
    /// Checks if the current user is a super admin (cross-tenant access)
    /// </summary>
    Task<bool> IsSuperAdminAsync();

    /// <summary>
    /// Gets all tenants (for super admin use)
    /// </summary>
    Task<List<Tenant>> GetAllTenantsAsync();

    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    Task<Tenant?> GetTenantByIdAsync(int tenantId);

    /// <summary>
    /// Creates a new tenant with default settings
    /// </summary>
    Task<Tenant> CreateTenantAsync(Tenant tenant);

    /// <summary>
    /// Updates an existing tenant
    /// </summary>
    Task<Tenant> UpdateTenantAsync(Tenant tenant);

    /// <summary>
    /// Deactivates a tenant (soft delete)
    /// </summary>
    Task<bool> DeactivateTenantAsync(int tenantId);

    /// <summary>
    /// Updates tenant settings
    /// </summary>
    Task<TenantSettings> UpdateTenantSettingsAsync(TenantSettings settings);

    /// <summary>
    /// Checks if tenant can create more companies based on license limits
    /// </summary>
    Task<bool> CanCreateCompanyAsync(int tenantId);

    /// <summary>
    /// Checks if tenant can create more users based on license limits
    /// </summary>
    Task<bool> CanCreateUserAsync(int tenantId);
}
