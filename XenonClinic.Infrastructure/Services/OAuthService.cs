using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// OAuth/SSO authentication service implementation.
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly ILogger<OAuthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly OAuthSettings _settings;
    private readonly ClinicDbContext _dbContext;

    public OAuthService(
        ILogger<OAuthService> logger,
        HttpClient httpClient,
        IOptions<OAuthSettings> settings,
        ClinicDbContext dbContext)
    {
        _logger = logger;
        _httpClient = httpClient;
        _settings = settings.Value;
        _dbContext = dbContext;
    }

    public Task<string> GetAuthorizationUrlAsync(OAuthProvider provider, string redirectUri, string? state = null)
    {
        var config = GetProviderConfig(provider);
        state ??= GenerateState();

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = string.Join(" ", config.Scopes),
            ["state"] = state
        };

        // Provider-specific parameters
        if (provider == OAuthProvider.Google)
        {
            queryParams["access_type"] = "offline";
            queryParams["prompt"] = "consent";
        }

        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        var url = $"{config.AuthorizationEndpoint}?{queryString}";

        _logger.LogInformation("Generated OAuth URL for {Provider}", provider);
        return Task.FromResult(url);
    }

    public async Task<OAuthTokenResult> ExchangeCodeAsync(OAuthProvider provider, string code, string redirectUri)
    {
        var config = GetProviderConfig(provider);

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret
        };

        var response = await _httpClient.PostAsync(
            config.TokenEndpoint,
            new FormUrlEncodedContent(tokenRequest));

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var result = new OAuthTokenResult(
            json.GetProperty("access_token").GetString()!,
            json.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            json.TryGetProperty("id_token", out var it) ? it.GetString() : null,
            json.TryGetProperty("token_type", out var tt) ? tt.GetString()! : "Bearer",
            json.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 3600,
            json.TryGetProperty("scope", out var s) ? s.GetString() : null
        );

        _logger.LogInformation("OAuth code exchanged successfully for {Provider}", provider);
        return result;
    }

    public async Task<OAuthTokenResult> RefreshTokenAsync(OAuthProvider provider, string refreshToken)
    {
        var config = GetProviderConfig(provider);

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret
        };

        var response = await _httpClient.PostAsync(
            config.TokenEndpoint,
            new FormUrlEncodedContent(tokenRequest));

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return new OAuthTokenResult(
            json.GetProperty("access_token").GetString()!,
            json.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshToken,
            json.TryGetProperty("id_token", out var it) ? it.GetString() : null,
            json.TryGetProperty("token_type", out var tt) ? tt.GetString()! : "Bearer",
            json.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 3600,
            json.TryGetProperty("scope", out var s) ? s.GetString() : null
        );
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(OAuthProvider provider, string accessToken)
    {
        var config = GetProviderConfig(provider);

        var request = new HttpRequestMessage(HttpMethod.Get, config.UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var userInfo = provider switch
        {
            OAuthProvider.Google => ParseGoogleUserInfo(json),
            OAuthProvider.Microsoft => ParseMicrosoftUserInfo(json),
            OAuthProvider.GitHub => ParseGitHubUserInfo(json),
            _ => ParseGenericUserInfo(json)
        };

        _logger.LogInformation("Retrieved user info from {Provider}: {Email}", provider, userInfo.Email);
        return userInfo;
    }

    public async Task RevokeTokenAsync(OAuthProvider provider, string token)
    {
        var config = GetProviderConfig(provider);

        var revokeEndpoint = provider switch
        {
            OAuthProvider.Google => "https://oauth2.googleapis.com/revoke",
            OAuthProvider.Microsoft => $"https://login.microsoftonline.com/common/oauth2/v2.0/logout",
            _ => null
        };

        if (revokeEndpoint == null)
        {
            _logger.LogWarning("Token revocation not supported for {Provider}", provider);
            return;
        }

        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
        await _httpClient.PostAsync(revokeEndpoint, content);

        _logger.LogInformation("Token revoked for {Provider}", provider);
    }

    public Task<OAuthUserInfo?> ValidateIdTokenAsync(OAuthProvider provider, string idToken)
    {
        try
        {
            // Parse JWT (simplified - in production, validate signature using JWKS)
            var parts = idToken.Split('.');
            if (parts.Length != 3) return Task.FromResult<OAuthUserInfo?>(null);

            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var jsonBytes = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
            var json = JsonSerializer.Deserialize<JsonElement>(jsonBytes);

            var claims = new Dictionary<string, object>();
            foreach (var prop in json.EnumerateObject())
            {
                claims[prop.Name] = prop.Value.ToString();
            }

            var userInfo = new OAuthUserInfo(
                json.TryGetProperty("sub", out var sub) ? sub.GetString()! : "",
                json.TryGetProperty("email", out var email) ? email.GetString()! : "",
                json.TryGetProperty("email_verified", out var ev) && ev.GetBoolean(),
                json.TryGetProperty("name", out var name) ? name.GetString() : null,
                json.TryGetProperty("given_name", out var fn) ? fn.GetString() : null,
                json.TryGetProperty("family_name", out var ln) ? ln.GetString() : null,
                json.TryGetProperty("picture", out var pic) ? pic.GetString() : null,
                json.TryGetProperty("locale", out var locale) ? locale.GetString() : null,
                claims
            );

            return Task.FromResult<OAuthUserInfo?>(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate ID token");
            return Task.FromResult<OAuthUserInfo?>(null);
        }
    }

    public async Task LinkAccountAsync(string userId, OAuthProvider provider, string providerUserId, string? accessToken = null)
    {
        var providerName = provider.ToString();

        // Remove existing link for this provider
        var existingLink = await _dbContext.OAuthLinkedAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == providerName);

        if (existingLink != null)
        {
            existingLink.ProviderUserId = providerUserId;
            existingLink.UpdatedAt = DateTime.UtcNow;
            existingLink.LastUsedAt = DateTime.UtcNow;
        }
        else
        {
            var newLink = new OAuthLinkedAccount
            {
                UserId = userId,
                Provider = providerName,
                ProviderUserId = providerUserId,
                LinkedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.OAuthLinkedAccounts.Add(newLink);
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Linked {Provider} account for user {UserId}", provider, userId);
    }

    public async Task UnlinkAccountAsync(string userId, OAuthProvider provider)
    {
        var providerName = provider.ToString();
        var linkedAccount = await _dbContext.OAuthLinkedAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == providerName);

        if (linkedAccount != null)
        {
            _dbContext.OAuthLinkedAccounts.Remove(linkedAccount);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Unlinked {Provider} account for user {UserId}", provider, userId);
    }

    public async Task<IEnumerable<LinkedOAuthAccount>> GetLinkedAccountsAsync(string userId)
    {
        var dbAccounts = await _dbContext.OAuthLinkedAccounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return dbAccounts.Select(a => new LinkedOAuthAccount(
            Enum.Parse<OAuthProvider>(a.Provider),
            a.ProviderUserId,
            a.ProviderEmail,
            a.LinkedAt
        ));
    }

    #region Private Methods

    private OAuthProviderConfig GetProviderConfig(OAuthProvider provider)
    {
        return provider switch
        {
            OAuthProvider.Google => new OAuthProviderConfig
            {
                Provider = OAuthProvider.Google,
                ClientId = _settings.Google.ClientId,
                ClientSecret = _settings.Google.ClientSecret,
                AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
                TokenEndpoint = "https://oauth2.googleapis.com/token",
                UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo",
                JwksUri = "https://www.googleapis.com/oauth2/v3/certs",
                Scopes = new[] { "openid", "email", "profile" }
            },
            OAuthProvider.Microsoft => new OAuthProviderConfig
            {
                Provider = OAuthProvider.Microsoft,
                ClientId = _settings.Microsoft.ClientId,
                ClientSecret = _settings.Microsoft.ClientSecret,
                AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
                TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token",
                UserInfoEndpoint = "https://graph.microsoft.com/v1.0/me",
                Scopes = new[] { "openid", "email", "profile", "User.Read" }
            },
            OAuthProvider.GitHub => new OAuthProviderConfig
            {
                Provider = OAuthProvider.GitHub,
                ClientId = _settings.GitHub.ClientId,
                ClientSecret = _settings.GitHub.ClientSecret,
                AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                TokenEndpoint = "https://github.com/login/oauth/access_token",
                UserInfoEndpoint = "https://api.github.com/user",
                Scopes = new[] { "user:email" }
            },
            _ => throw new NotSupportedException($"OAuth provider {provider} is not configured")
        };
    }

    private static string GenerateState()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static OAuthUserInfo ParseGoogleUserInfo(JsonElement json)
    {
        return new OAuthUserInfo(
            json.GetProperty("sub").GetString()!,
            json.GetProperty("email").GetString()!,
            json.TryGetProperty("email_verified", out var ev) && ev.GetBoolean(),
            json.TryGetProperty("name", out var name) ? name.GetString() : null,
            json.TryGetProperty("given_name", out var fn) ? fn.GetString() : null,
            json.TryGetProperty("family_name", out var ln) ? ln.GetString() : null,
            json.TryGetProperty("picture", out var pic) ? pic.GetString() : null,
            json.TryGetProperty("locale", out var locale) ? locale.GetString() : null,
            new Dictionary<string, object>()
        );
    }

    private static OAuthUserInfo ParseMicrosoftUserInfo(JsonElement json)
    {
        return new OAuthUserInfo(
            json.GetProperty("id").GetString()!,
            json.TryGetProperty("mail", out var mail) ? mail.GetString()! :
                json.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString()! : "",
            true, // Microsoft doesn't return email_verified
            json.TryGetProperty("displayName", out var name) ? name.GetString() : null,
            json.TryGetProperty("givenName", out var fn) ? fn.GetString() : null,
            json.TryGetProperty("surname", out var ln) ? ln.GetString() : null,
            null,
            json.TryGetProperty("preferredLanguage", out var lang) ? lang.GetString() : null,
            new Dictionary<string, object>()
        );
    }

    private static OAuthUserInfo ParseGitHubUserInfo(JsonElement json)
    {
        return new OAuthUserInfo(
            json.GetProperty("id").GetInt64().ToString(),
            json.TryGetProperty("email", out var email) ? email.GetString()! : "",
            true,
            json.TryGetProperty("name", out var name) ? name.GetString() : null,
            null,
            null,
            json.TryGetProperty("avatar_url", out var pic) ? pic.GetString() : null,
            null,
            new Dictionary<string, object>
            {
                ["login"] = json.TryGetProperty("login", out var login) ? login.GetString()! : ""
            }
        );
    }

    private static OAuthUserInfo ParseGenericUserInfo(JsonElement json)
    {
        var claims = new Dictionary<string, object>();
        foreach (var prop in json.EnumerateObject())
        {
            claims[prop.Name] = prop.Value.ToString();
        }

        return new OAuthUserInfo(
            json.TryGetProperty("sub", out var sub) ? sub.GetString()! :
                json.TryGetProperty("id", out var id) ? id.ToString() : "",
            json.TryGetProperty("email", out var email) ? email.GetString()! : "",
            json.TryGetProperty("email_verified", out var ev) && ev.GetBoolean(),
            json.TryGetProperty("name", out var name) ? name.GetString() : null,
            null,
            null,
            null,
            null,
            claims
        );
    }

    #endregion
}

/// <summary>
/// OAuth settings configuration.
/// </summary>
public class OAuthSettings
{
    public OAuthProviderSettings Google { get; set; } = new();
    public OAuthProviderSettings Microsoft { get; set; } = new();
    public OAuthProviderSettings GitHub { get; set; } = new();
}

/// <summary>
/// Individual OAuth provider settings.
/// </summary>
public class OAuthProviderSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
