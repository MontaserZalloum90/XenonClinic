using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for managing company authentication configuration
/// </summary>
public class CompanyAuthConfigService : ICompanyAuthConfigService
{
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<CompanyAuthConfigService> _logger;

    public CompanyAuthConfigService(
        ClinicDbContext dbContext,
        ILogger<CompanyAuthConfigService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CompanyAuthSettings?> GetAuthSettingsAsync(int companyId)
    {
        return await _dbContext.CompanyAuthSettings
            .Include(s => s.IdentityProviders.Where(p => p.IsEnabled))
            .FirstOrDefaultAsync(s => s.CompanyId == companyId);
    }

    /// <inheritdoc />
    public async Task<CompanyAuthSettings?> GetAuthSettingsByCodeAsync(string companyCode)
    {
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Code == companyCode && c.IsActive);

        if (company == null)
            return null;

        return await GetAuthSettingsAsync(company.Id);
    }

    /// <inheritdoc />
    public async Task<IList<CompanyIdentityProvider>> GetIdentityProvidersAsync(int companyId)
    {
        return await _dbContext.CompanyIdentityProviders
            .Where(p => p.CompanyId == companyId && p.IsEnabled)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.DisplayName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CompanyIdentityProvider?> GetIdentityProviderAsync(int companyId, string providerName)
    {
        return await _dbContext.CompanyIdentityProviders
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId &&
                                       p.Name == providerName &&
                                       p.IsEnabled);
    }

    /// <inheritdoc />
    public async Task<CompanyIdentityProvider?> GetIdentityProviderByIdAsync(int providerId)
    {
        return await _dbContext.CompanyIdentityProviders
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == providerId);
    }

    /// <inheritdoc />
    public async Task<CompanyIdentityProvider?> GetDefaultIdentityProviderAsync(int companyId)
    {
        // First try to find the default provider
        var defaultProvider = await _dbContext.CompanyIdentityProviders
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.CompanyId == companyId &&
                                       p.IsDefault &&
                                       p.IsEnabled);

        if (defaultProvider != null)
            return defaultProvider;

        // Fallback to first enabled provider
        return await _dbContext.CompanyIdentityProviders
            .Include(p => p.Company)
            .Where(p => p.CompanyId == companyId && p.IsEnabled)
            .OrderBy(p => p.DisplayOrder)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<bool> IsLocalLoginAllowedAsync(int companyId)
    {
        var settings = await GetAuthSettingsAsync(companyId);

        // Default behavior: local login is allowed
        if (settings == null || !settings.IsEnabled)
            return true;

        return settings.AllowLocalLogin;
    }

    /// <inheritdoc />
    public async Task<bool> IsExternalLoginAllowedAsync(int companyId)
    {
        var settings = await GetAuthSettingsAsync(companyId);

        // Default behavior: external login is not allowed unless explicitly configured
        if (settings == null || !settings.IsEnabled)
            return false;

        return settings.AllowExternalLogin;
    }

    /// <inheritdoc />
    public async Task<CompanyAuthSettings> SaveAuthSettingsAsync(CompanyAuthSettings settings)
    {
        var existing = await _dbContext.CompanyAuthSettings
            .FirstOrDefaultAsync(s => s.CompanyId == settings.CompanyId);

        if (existing != null)
        {
            // Update existing
            existing.AuthMode = settings.AuthMode;
            existing.AllowLocalLogin = settings.AllowLocalLogin;
            existing.AllowExternalLogin = settings.AllowExternalLogin;
            existing.AutoProvisionUsers = settings.AutoProvisionUsers;
            existing.DefaultRoleOnAutoProvision = settings.DefaultRoleOnAutoProvision;
            existing.DefaultExternalProviderName = settings.DefaultExternalProviderName;
            existing.IsEnabled = settings.IsEnabled;
            existing.LoginPageMessage = settings.LoginPageMessage;
            existing.LoginPageMessageAr = settings.LoginPageMessageAr;
            existing.PostLoginRedirectUrl = settings.PostLoginRedirectUrl;
            existing.PostLogoutRedirectUrl = settings.PostLogoutRedirectUrl;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = settings.UpdatedBy;

            _logger.LogInformation("Updated auth settings for company {CompanyId}", settings.CompanyId);
        }
        else
        {
            // Create new
            settings.CreatedAt = DateTime.UtcNow;
            _dbContext.CompanyAuthSettings.Add(settings);
            existing = settings;

            _logger.LogInformation("Created auth settings for company {CompanyId}", settings.CompanyId);
        }

        await _dbContext.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc />
    public async Task<CompanyIdentityProvider> SaveIdentityProviderAsync(CompanyIdentityProvider provider)
    {
        var existing = provider.Id > 0
            ? await _dbContext.CompanyIdentityProviders.FindAsync(provider.Id)
            : null;

        if (existing != null)
        {
            // Update existing provider
            existing.Name = provider.Name;
            existing.DisplayName = provider.DisplayName;
            existing.Type = provider.Type;
            existing.IsDefault = provider.IsDefault;
            existing.IsEnabled = provider.IsEnabled;
            existing.DisplayOrder = provider.DisplayOrder;
            existing.IconClass = provider.IconClass;
            existing.ButtonClass = provider.ButtonClass;

            // OIDC settings
            existing.OidcAuthority = provider.OidcAuthority;
            existing.OidcClientId = provider.OidcClientId;
            existing.OidcClientSecretEncrypted = provider.OidcClientSecretEncrypted;
            existing.OidcCallbackPath = provider.OidcCallbackPath;
            existing.OidcSignedOutCallbackPath = provider.OidcSignedOutCallbackPath;
            existing.OidcResponseType = provider.OidcResponseType;
            existing.OidcUsePkce = provider.OidcUsePkce;
            existing.OidcScopes = provider.OidcScopes;
            existing.OidcRequireHttpsMetadata = provider.OidcRequireHttpsMetadata;
            existing.OidcGetClaimsFromUserInfoEndpoint = provider.OidcGetClaimsFromUserInfoEndpoint;

            // SAML settings
            existing.SamlEntityId = provider.SamlEntityId;
            existing.SamlAcsPath = provider.SamlAcsPath;
            existing.SamlSloPath = provider.SamlSloPath;
            existing.SamlMetadataUrl = provider.SamlMetadataUrl;
            existing.SamlIdpEntityId = provider.SamlIdpEntityId;
            existing.SamlSignAuthnRequests = provider.SamlSignAuthnRequests;
            existing.SamlSpCertificate = provider.SamlSpCertificate;
            existing.SamlSpCertificatePasswordEncrypted = provider.SamlSpCertificatePasswordEncrypted;
            existing.SamlWantAssertionsSigned = provider.SamlWantAssertionsSigned;
            existing.SamlAllowedClockSkewMinutes = provider.SamlAllowedClockSkewMinutes;

            // WS-Fed settings
            existing.WsFedMetadataAddress = provider.WsFedMetadataAddress;
            existing.WsFedWtrealm = provider.WsFedWtrealm;
            existing.WsFedReplyUrl = provider.WsFedReplyUrl;
            existing.WsFedRequireHttps = provider.WsFedRequireHttps;

            // Claim mappings
            existing.ClaimMappingEmail = provider.ClaimMappingEmail;
            existing.ClaimMappingUpn = provider.ClaimMappingUpn;
            existing.ClaimMappingName = provider.ClaimMappingName;
            existing.ClaimMappingFirstName = provider.ClaimMappingFirstName;
            existing.ClaimMappingLastName = provider.ClaimMappingLastName;
            existing.ClaimMappingGroups = provider.ClaimMappingGroups;
            existing.ExtraConfigJson = provider.ExtraConfigJson;

            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = provider.UpdatedBy;

            _logger.LogInformation("Updated identity provider {ProviderName} for company {CompanyId}",
                provider.Name, provider.CompanyId);
        }
        else
        {
            // Create new provider
            provider.CreatedAt = DateTime.UtcNow;
            _dbContext.CompanyIdentityProviders.Add(provider);
            existing = provider;

            _logger.LogInformation("Created identity provider {ProviderName} for company {CompanyId}",
                provider.Name, provider.CompanyId);
        }

        // If this provider is set as default, unset other defaults for the same company
        if (existing.IsDefault)
        {
            await _dbContext.CompanyIdentityProviders
                .Where(p => p.CompanyId == existing.CompanyId && p.Id != existing.Id && p.IsDefault)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.IsDefault, false));
        }

        await _dbContext.SaveChangesAsync();
        return existing;
    }

    /// <inheritdoc />
    public async Task DeleteIdentityProviderAsync(int providerId)
    {
        var provider = await _dbContext.CompanyIdentityProviders.FindAsync(providerId);
        if (provider != null)
        {
            _dbContext.CompanyIdentityProviders.Remove(provider);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted identity provider {ProviderId}", providerId);
        }
    }

    /// <inheritdoc />
    public async Task<AuthMode> GetEffectiveAuthModeAsync(int companyId)
    {
        var settings = await GetAuthSettingsAsync(companyId);

        // Default to LocalOnly if no settings configured
        if (settings == null || !settings.IsEnabled)
            return AuthMode.LocalOnly;

        return settings.AuthMode;
    }
}
