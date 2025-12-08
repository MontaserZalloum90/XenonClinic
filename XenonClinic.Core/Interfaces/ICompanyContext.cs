using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for detecting and resolving company context at login time
/// </summary>
public interface ICompanyContext
{
    /// <summary>
    /// Attempts to detect the current company from the HTTP request context
    /// Uses multiple strategies: subdomain, path parameter, query string, cookie
    /// </summary>
    /// <returns>The detected company or null if not found</returns>
    Task<Company?> GetCurrentCompanyAsync();

    /// <summary>
    /// Gets the company by code
    /// </summary>
    /// <param name="companyCode">The company code</param>
    /// <returns>The company or null if not found</returns>
    Task<Company?> GetCompanyByCodeAsync(string companyCode);

    /// <summary>
    /// Gets the company by ID
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>The company or null if not found</returns>
    Task<Company?> GetCompanyByIdAsync(int companyId);

    /// <summary>
    /// Sets the current company context in the session/cookie
    /// </summary>
    /// <param name="companyCode">The company code to set</param>
    void SetCurrentCompany(string companyCode);

    /// <summary>
    /// Clears the company context from the session/cookie
    /// </summary>
    void ClearCurrentCompany();

    /// <summary>
    /// Gets all active companies (for company selection dropdown)
    /// </summary>
    /// <returns>List of active companies</returns>
    Task<IList<Company>> GetAllActiveCompaniesAsync();
}
