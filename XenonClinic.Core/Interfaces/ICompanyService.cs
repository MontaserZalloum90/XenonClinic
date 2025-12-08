using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for company-level operations and context management
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Gets the current user's company ID
    /// </summary>
    Task<int?> GetCurrentCompanyIdAsync();

    /// <summary>
    /// Gets the current company entity
    /// </summary>
    Task<Company?> GetCurrentCompanyAsync();

    /// <summary>
    /// Checks if the current user has access to a specific company
    /// </summary>
    Task<bool> HasAccessToCompanyAsync(int companyId);

    /// <summary>
    /// Gets all companies for the current tenant
    /// </summary>
    Task<List<Company>> GetTenantCompaniesAsync();

    /// <summary>
    /// Gets all companies accessible by the current user
    /// </summary>
    Task<List<Company>> GetUserCompaniesAsync();

    /// <summary>
    /// Gets a company by ID
    /// </summary>
    Task<Company?> GetCompanyByIdAsync(int companyId);

    /// <summary>
    /// Creates a new company within a tenant
    /// </summary>
    Task<Company> CreateCompanyAsync(Company company);

    /// <summary>
    /// Updates an existing company
    /// </summary>
    Task<Company> UpdateCompanyAsync(Company company);

    /// <summary>
    /// Deactivates a company (soft delete)
    /// </summary>
    Task<bool> DeactivateCompanyAsync(int companyId);

    /// <summary>
    /// Checks if company can create more branches based on tenant limits
    /// </summary>
    Task<bool> CanCreateBranchAsync(int companyId);

    /// <summary>
    /// Gets branches for a specific company
    /// </summary>
    Task<List<Branch>> GetCompanyBranchesAsync(int companyId);
}
