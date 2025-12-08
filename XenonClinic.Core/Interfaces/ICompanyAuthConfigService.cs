using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for managing company authentication configuration
/// </summary>
public interface ICompanyAuthConfigService
{
    /// <summary>
    /// Gets the authentication settings for a company
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>Auth settings or null if not configured (defaults to local-only)</returns>
    Task<CompanyAuthSettings?> GetAuthSettingsAsync(int companyId);

    /// <summary>
    /// Gets the authentication settings for a company by code
    /// </summary>
    /// <param name="companyCode">The company code</param>
    /// <returns>Auth settings or null if not configured</returns>
    Task<CompanyAuthSettings?> GetAuthSettingsByCodeAsync(string companyCode);

    /// <summary>
    /// Gets all enabled identity providers for a company
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>List of enabled identity providers</returns>
    Task<IList<CompanyIdentityProvider>> GetIdentityProvidersAsync(int companyId);

    /// <summary>
    /// Gets a specific identity provider by name
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <param name="providerName">The provider name</param>
    /// <returns>The identity provider or null</returns>
    Task<CompanyIdentityProvider?> GetIdentityProviderAsync(int companyId, string providerName);

    /// <summary>
    /// Gets a specific identity provider by ID
    /// </summary>
    /// <param name="providerId">The provider ID</param>
    /// <returns>The identity provider or null</returns>
    Task<CompanyIdentityProvider?> GetIdentityProviderByIdAsync(int providerId);

    /// <summary>
    /// Gets the default identity provider for a company
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>The default provider or first enabled provider</returns>
    Task<CompanyIdentityProvider?> GetDefaultIdentityProviderAsync(int companyId);

    /// <summary>
    /// Checks if local login is allowed for the company
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>True if local login is allowed</returns>
    Task<bool> IsLocalLoginAllowedAsync(int companyId);

    /// <summary>
    /// Checks if external login is allowed for the company
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>True if external login is allowed</returns>
    Task<bool> IsExternalLoginAllowedAsync(int companyId);

    /// <summary>
    /// Creates or updates auth settings for a company
    /// </summary>
    /// <param name="settings">The auth settings to save</param>
    /// <returns>The saved auth settings</returns>
    Task<CompanyAuthSettings> SaveAuthSettingsAsync(CompanyAuthSettings settings);

    /// <summary>
    /// Creates or updates an identity provider configuration
    /// </summary>
    /// <param name="provider">The provider configuration to save</param>
    /// <returns>The saved provider configuration</returns>
    Task<CompanyIdentityProvider> SaveIdentityProviderAsync(CompanyIdentityProvider provider);

    /// <summary>
    /// Deletes an identity provider configuration
    /// </summary>
    /// <param name="providerId">The provider ID to delete</param>
    Task DeleteIdentityProviderAsync(int providerId);

    /// <summary>
    /// Gets effective auth mode for a company (with defaults)
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <returns>The effective auth mode</returns>
    Task<AuthMode> GetEffectiveAuthModeAsync(int companyId);
}
