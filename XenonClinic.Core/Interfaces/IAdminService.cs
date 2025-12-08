using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for admin-level operations across the entire system
/// </summary>
public interface IAdminService
{
    // ==================== Tenant Management ====================

    /// <summary>
    /// Gets all tenants with optional filtering
    /// </summary>
    Task<List<Tenant>> GetTenantsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets tenant statistics (company count, branch count, user count)
    /// </summary>
    Task<TenantStatistics> GetTenantStatisticsAsync(int tenantId);

    // ==================== Company Management ====================

    /// <summary>
    /// Gets all companies for a tenant
    /// </summary>
    Task<List<Company>> GetCompaniesAsync(int tenantId, bool includeInactive = false);

    /// <summary>
    /// Gets company statistics (branch count, user count)
    /// </summary>
    Task<CompanyStatistics> GetCompanyStatisticsAsync(int companyId);

    // ==================== Branch Management ====================

    /// <summary>
    /// Gets all branches for a company
    /// </summary>
    Task<List<Branch>> GetBranchesAsync(int companyId, bool includeInactive = false);

    /// <summary>
    /// Creates a new branch
    /// </summary>
    Task<Branch> CreateBranchAsync(Branch branch);

    /// <summary>
    /// Updates an existing branch
    /// </summary>
    Task<Branch> UpdateBranchAsync(Branch branch);

    /// <summary>
    /// Deactivates a branch
    /// </summary>
    Task<bool> DeactivateBranchAsync(int branchId);

    // ==================== User Management ====================

    /// <summary>
    /// Gets all users for a tenant
    /// </summary>
    Task<List<ApplicationUser>> GetTenantUsersAsync(int tenantId, bool includeInactive = false);

    /// <summary>
    /// Gets all users for a company
    /// </summary>
    Task<List<ApplicationUser>> GetCompanyUsersAsync(int companyId, bool includeInactive = false);

    /// <summary>
    /// Gets all users for a branch
    /// </summary>
    Task<List<ApplicationUser>> GetBranchUsersAsync(int branchId, bool includeInactive = false);

    /// <summary>
    /// Creates a new user with specified tenant/company/branch
    /// </summary>
    Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password, List<string> roles, List<int> branchIds);

    /// <summary>
    /// Updates a user's tenant/company/branch assignments
    /// </summary>
    Task<ApplicationUser> UpdateUserAssignmentsAsync(string userId, int? tenantId, int? companyId, int? primaryBranchId, List<int> branchIds);

    /// <summary>
    /// Deactivates a user
    /// </summary>
    Task<bool> DeactivateUserAsync(string userId);

    /// <summary>
    /// Gets user roles
    /// </summary>
    Task<List<string>> GetUserRolesAsync(string userId);

    /// <summary>
    /// Updates user roles
    /// </summary>
    Task UpdateUserRolesAsync(string userId, List<string> roles);

    // ==================== System Statistics ====================

    /// <summary>
    /// Gets overall system statistics
    /// </summary>
    Task<SystemStatistics> GetSystemStatisticsAsync();
}

/// <summary>
/// Statistics for a tenant
/// </summary>
public class TenantStatistics
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int CompanyCount { get; set; }
    public int BranchCount { get; set; }
    public int UserCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int PatientCount { get; set; }
}

/// <summary>
/// Statistics for a company
/// </summary>
public class CompanyStatistics
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int BranchCount { get; set; }
    public int UserCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int PatientCount { get; set; }
}

/// <summary>
/// Overall system statistics
/// </summary>
public class SystemStatistics
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalBranches { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalPatients { get; set; }
}
