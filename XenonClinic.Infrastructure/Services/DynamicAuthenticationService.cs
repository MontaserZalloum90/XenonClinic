using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for dynamic authentication configuration and handling
/// </summary>
public class DynamicAuthenticationService : IDynamicAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClinicDbContext _dbContext;
    private readonly ILogger<DynamicAuthenticationService> _logger;

    // Base paths for SSO callbacks
    private const string OidcCallbackBasePath = "/auth/oidc/callback";
    private const string SamlAcsBasePath = "/auth/saml/acs";
    private const string WsFedCallbackBasePath = "/auth/wsfed/callback";
    private const string ExternalLoginPath = "/Account/ExternalLogin";

    public DynamicAuthenticationService(
        IHttpContextAccessor httpContextAccessor,
        ClinicDbContext dbContext,
        ILogger<DynamicAuthenticationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GetChallengeUrlAsync(int providerId, string? returnUrl = null)
    {
        var provider = await _dbContext.CompanyIdentityProviders
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == providerId && p.IsEnabled);

        if (provider == null)
        {
            _logger.LogWarning("Provider {ProviderId} not found or disabled", providerId);
            return "/Account/Login?error=provider_not_found";
        }

        var baseUrl = GetBaseUrl();
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? "/");

        // Build challenge URL based on provider type
        return provider.Type switch
        {
            IdentityProviderType.OIDC => $"{baseUrl}{ExternalLoginPath}?provider={providerId}&returnUrl={encodedReturnUrl}",
            IdentityProviderType.SAML2 => $"{baseUrl}{ExternalLoginPath}?provider={providerId}&returnUrl={encodedReturnUrl}",
            IdentityProviderType.WSFED => $"{baseUrl}{ExternalLoginPath}?provider={providerId}&returnUrl={encodedReturnUrl}",
            _ => throw new InvalidOperationException($"Unsupported provider type: {provider.Type}")
        };
    }

    /// <inheritdoc />
    public string GetSchemeNameForProvider(int providerId)
    {
        return $"ExternalAuth.{providerId}";
    }

    /// <inheritdoc />
    public string GetSchemeNameForProvider(int companyId, string providerName)
    {
        return $"ExternalAuth.{companyId}.{providerName}";
    }

    /// <inheritdoc />
    public string GetOidcCallbackUrl(int providerId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}{OidcCallbackBasePath}/{providerId}";
    }

    /// <inheritdoc />
    public string GetSamlAcsUrl(int providerId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}{SamlAcsBasePath}/{providerId}";
    }

    /// <inheritdoc />
    public string GetWsFedReplyUrl(int providerId)
    {
        var baseUrl = GetBaseUrl();
        return $"{baseUrl}{WsFedCallbackBasePath}/{providerId}";
    }

    /// <inheritdoc />
    public async Task<IList<string>> GetAllCallbackPathsAsync()
    {
        var providerIds = await _dbContext.CompanyIdentityProviders
            .Where(p => p.IsEnabled)
            .Select(p => p.Id)
            .ToListAsync();

        var paths = new List<string>();

        foreach (var id in providerIds)
        {
            paths.Add($"{OidcCallbackBasePath}/{id}");
            paths.Add($"{SamlAcsBasePath}/{id}");
            paths.Add($"{WsFedCallbackBasePath}/{id}");
        }

        return paths;
    }

    private string GetBaseUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return string.Empty;

        var request = httpContext.Request;
        return $"{request.Scheme}://{request.Host}";
    }
}
