namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for resolving theme colors with company override support.
/// Company colors take precedence over tenant colors.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the effective theme colors for a company (with fallback to tenant).
    /// </summary>
    Task<ThemeColors> GetCompanyThemeColorsAsync(int companyId);

    /// <summary>
    /// Gets the effective theme colors for a branch (with fallback to company, then tenant).
    /// </summary>
    Task<ThemeColors> GetBranchThemeColorsAsync(int branchId);

    /// <summary>
    /// Gets the tenant's theme colors.
    /// </summary>
    Task<ThemeColors> GetTenantThemeColorsAsync(int tenantId);

    /// <summary>
    /// Updates tenant theme colors.
    /// </summary>
    Task UpdateTenantThemeColorsAsync(int tenantId, string primaryColor, string secondaryColor);

    /// <summary>
    /// Updates company theme colors (null to use tenant default).
    /// </summary>
    Task UpdateCompanyThemeColorsAsync(int companyId, string? primaryColor, string? secondaryColor);

    /// <summary>
    /// Updates branch theme colors (null to use company/tenant default).
    /// </summary>
    Task UpdateBranchThemeColorsAsync(int branchId, string? primaryColor, string? secondaryColor);

    /// <summary>
    /// Resets company theme colors to use tenant defaults.
    /// </summary>
    Task ResetCompanyThemeColorsAsync(int companyId);

    /// <summary>
    /// Resets branch theme colors to use company/tenant defaults.
    /// </summary>
    Task ResetBranchThemeColorsAsync(int branchId);
}

/// <summary>
/// Resolved theme colors.
/// </summary>
public class ThemeColors
{
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public string Source { get; set; } = "Default"; // "Tenant", "Company", "Branch", or "Default"
}
