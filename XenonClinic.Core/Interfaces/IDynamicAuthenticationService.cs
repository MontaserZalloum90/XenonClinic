namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Result of external authentication operation
/// </summary>
public class ExternalAuthResult
{
    public bool Succeeded { get; set; }
    public IApplicationUser? User { get; set; }
    public bool IsNewUser { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }

    public static ExternalAuthResult Success(IApplicationUser user, bool isNew, string? returnUrl = null)
    {
        return new ExternalAuthResult
        {
            Succeeded = true,
            User = user,
            IsNewUser = isNew,
            ReturnUrl = returnUrl
        };
    }

    public static ExternalAuthResult Failure(string error, string? description = null)
    {
        return new ExternalAuthResult
        {
            Succeeded = false,
            Error = error,
            ErrorDescription = description
        };
    }
}

/// <summary>
/// Service for dynamic authentication configuration and handling
/// </summary>
public interface IDynamicAuthenticationService
{
    /// <summary>
    /// Gets the challenge URL for initiating external authentication
    /// </summary>
    /// <param name="providerId">The identity provider ID</param>
    /// <param name="returnUrl">The URL to return to after authentication</param>
    /// <returns>The challenge URL</returns>
    Task<string> GetChallengeUrlAsync(int providerId, string? returnUrl = null);

    /// <summary>
    /// Builds the authentication scheme name for a provider
    /// </summary>
    /// <param name="providerId">The provider ID</param>
    /// <returns>The unique scheme name</returns>
    string GetSchemeNameForProvider(int providerId);

    /// <summary>
    /// Builds the authentication scheme name for a provider
    /// </summary>
    /// <param name="companyId">The company ID</param>
    /// <param name="providerName">The provider name</param>
    /// <returns>The unique scheme name</returns>
    string GetSchemeNameForProvider(int companyId, string providerName);

    /// <summary>
    /// Gets the callback URL for OIDC providers
    /// </summary>
    /// <param name="providerId">The provider ID</param>
    /// <returns>The callback URL</returns>
    string GetOidcCallbackUrl(int providerId);

    /// <summary>
    /// Gets the SAML ACS URL for SAML providers
    /// </summary>
    /// <param name="providerId">The provider ID</param>
    /// <returns>The ACS URL</returns>
    string GetSamlAcsUrl(int providerId);

    /// <summary>
    /// Gets the WS-Fed reply URL
    /// </summary>
    /// <param name="providerId">The provider ID</param>
    /// <returns>The reply URL</returns>
    string GetWsFedReplyUrl(int providerId);

    /// <summary>
    /// Gets all configured callback paths for dynamic registration
    /// </summary>
    /// <returns>List of callback paths</returns>
    Task<IList<string>> GetAllCallbackPathsAsync();
}
