namespace XenonClinic.Core.Interfaces;

/// <summary>
/// OAuth/SSO authentication service interface.
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Get authorization URL for OAuth provider.
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(OAuthProvider provider, string redirectUri, string? state = null);

    /// <summary>
    /// Exchange authorization code for tokens.
    /// </summary>
    Task<OAuthTokenResult> ExchangeCodeAsync(OAuthProvider provider, string code, string redirectUri);

    /// <summary>
    /// Refresh access token.
    /// </summary>
    Task<OAuthTokenResult> RefreshTokenAsync(OAuthProvider provider, string refreshToken);

    /// <summary>
    /// Get user info from OAuth provider.
    /// </summary>
    Task<OAuthUserInfo> GetUserInfoAsync(OAuthProvider provider, string accessToken);

    /// <summary>
    /// Revoke token.
    /// </summary>
    Task RevokeTokenAsync(OAuthProvider provider, string token);

    /// <summary>
    /// Validate and decode ID token (for OpenID Connect).
    /// </summary>
    Task<OAuthUserInfo?> ValidateIdTokenAsync(OAuthProvider provider, string idToken);

    /// <summary>
    /// Link OAuth account to existing user.
    /// </summary>
    Task LinkAccountAsync(string userId, OAuthProvider provider, string providerUserId, string? accessToken = null);

    /// <summary>
    /// Unlink OAuth account from user.
    /// </summary>
    Task UnlinkAccountAsync(string userId, OAuthProvider provider);

    /// <summary>
    /// Get linked OAuth providers for user.
    /// </summary>
    Task<IEnumerable<LinkedOAuthAccount>> GetLinkedAccountsAsync(string userId);
}

/// <summary>
/// Supported OAuth providers.
/// </summary>
public enum OAuthProvider
{
    Google = 1,
    Microsoft = 2,
    Apple = 3,
    Facebook = 4,
    GitHub = 5,
    Custom = 99
}

/// <summary>
/// OAuth token result.
/// </summary>
public record OAuthTokenResult(
    string AccessToken,
    string? RefreshToken,
    string? IdToken,
    string TokenType,
    int ExpiresIn,
    string? Scope
);

/// <summary>
/// OAuth user information.
/// </summary>
public record OAuthUserInfo(
    string Id,
    string Email,
    bool EmailVerified,
    string? Name,
    string? FirstName,
    string? LastName,
    string? Picture,
    string? Locale,
    Dictionary<string, object> Claims
);

/// <summary>
/// Linked OAuth account.
/// </summary>
public record LinkedOAuthAccount(
    OAuthProvider Provider,
    string ProviderUserId,
    string? Email,
    DateTime LinkedAt
);

/// <summary>
/// OAuth provider configuration.
/// </summary>
public class OAuthProviderConfig
{
    public OAuthProvider Provider { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
    public string? JwksUri { get; set; }
    public string[] Scopes { get; set; } = Array.Empty<string>();
}
