using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Web.Models.Account;

namespace XenonClinic.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICompanyContext _companyContext;
    private readonly ICompanyAuthConfigService _authConfigService;
    private readonly IExternalUserMapper _externalUserMapper;
    private readonly IDynamicAuthenticationService _dynamicAuthService;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICompanyContext companyContext,
        ICompanyAuthConfigService authConfigService,
        IExternalUserMapper externalUserMapper,
        IDynamicAuthenticationService dynamicAuthService,
        ISecretEncryptionService encryptionService,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _companyContext = companyContext;
        _authConfigService = authConfigService;
        _externalUserMapper = externalUserMapper;
        _dynamicAuthService = dynamicAuthService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null, string? company = null, string? error = null)
    {
        // If already authenticated, redirect
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl ?? "/");
        }

        // Try to detect company from context
        Company? currentCompany = null;
        if (!string.IsNullOrEmpty(company))
        {
            currentCompany = await _companyContext.GetCompanyByCodeAsync(company);
            if (currentCompany != null)
            {
                _companyContext.SetCurrentCompany(company);
            }
        }
        else
        {
            currentCompany = await _companyContext.GetCurrentCompanyAsync();
        }

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/"),
            ErrorMessage = error
        };

        if (currentCompany != null)
        {
            model.CompanyId = currentCompany.Id;
            model.CompanyName = currentCompany.Name;
            model.CompanyCode = currentCompany.Code;

            // Get auth settings for the company
            var authSettings = await _authConfigService.GetAuthSettingsAsync(currentCompany.Id);

            if (authSettings != null && authSettings.IsEnabled)
            {
                model.ShowLocalLogin = authSettings.AllowLocalLogin;
                model.ShowExternalLogin = authSettings.AllowExternalLogin;
                model.LoginPageMessage = authSettings.LoginPageMessage;
                model.LoginPageMessageAr = authSettings.LoginPageMessageAr;

                // Get available identity providers
                if (authSettings.AllowExternalLogin)
                {
                    var providers = await _authConfigService.GetIdentityProvidersAsync(currentCompany.Id);
                    model.ExternalProviders = providers.Select(p => new ExternalProviderViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        IconClass = p.IconClass ?? "bi bi-box-arrow-in-right",
                        ButtonClass = p.ButtonClass ?? "btn-outline-primary",
                        IsDefault = p.IsDefault
                    }).ToList();
                }
            }
            else
            {
                // Default: local login only
                model.ShowLocalLogin = true;
                model.ShowExternalLogin = false;
            }
        }
        else
        {
            // No company detected - show company selection or default local login
            model.ShowCompanySelector = true;
            model.ShowLocalLogin = true;
            model.ShowExternalLogin = false;
            model.AvailableCompanies = (await _companyContext.GetAllActiveCompaniesAsync())
                .Select(c => new CompanySelectionViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    TenantName = c.Tenant?.Name ?? ""
                }).ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.ReturnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Verify local login is allowed for the company
        if (model.CompanyId.HasValue)
        {
            var isLocalAllowed = await _authConfigService.IsLocalLoginAllowedAsync(model.CompanyId.Value);
            if (!isLocalAllowed)
            {
                ModelState.AddModelError(string.Empty, "Local login is not allowed for this company. Please use SSO.");
                return View(model);
            }
        }

        // Attempt to sign in
        var result = await _signInManager.PasswordSignInAsync(
            model.Email!,
            model.Password!,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in", model.Email);

            // Update last login timestamp
            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            return LocalRedirect(model.ReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2fa), new { model.ReturnUrl, model.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account locked out", model.Email);
            return RedirectToAction(nameof(Lockout));
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SelectCompany(string code, string? returnUrl = null)
    {
        var company = await _companyContext.GetCompanyByCodeAsync(code);
        if (company != null)
        {
            _companyContext.SetCurrentCompany(code);
        }

        return RedirectToAction(nameof(Login), new { returnUrl, company = code });
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLogin(int provider, string? returnUrl = null)
    {
        var identityProvider = await _authConfigService.GetIdentityProviderByIdAsync(provider);
        if (identityProvider == null || !identityProvider.IsEnabled)
        {
            return RedirectToAction(nameof(Login), new { error = "Provider not found or disabled" });
        }

        // Verify external login is allowed
        var isAllowed = await _authConfigService.IsExternalLoginAllowedAsync(identityProvider.CompanyId);
        if (!isAllowed)
        {
            return RedirectToAction(nameof(Login), new { error = "External login is not allowed for this company" });
        }

        // Store provider ID and return URL in state
        var state = new Dictionary<string, string>
        {
            { "providerId", provider.ToString() },
            { "returnUrl", returnUrl ?? "/" }
        };

        // Handle based on provider type
        return identityProvider.Type switch
        {
            IdentityProviderType.OIDC => await ChallengeOidc(identityProvider, returnUrl),
            IdentityProviderType.SAML2 => ChallengeSaml(identityProvider, returnUrl),
            IdentityProviderType.WSFED => ChallengeWsFed(identityProvider, returnUrl),
            _ => RedirectToAction(nameof(Login), new { error = "Unsupported provider type" })
        };
    }

    private async Task<IActionResult> ChallengeOidc(CompanyIdentityProvider provider, string? returnUrl)
    {
        // Build OIDC options dynamically
        var schemeName = _dynamicAuthService.GetSchemeNameForProvider(provider.Id);
        var callbackPath = $"/auth/oidc/callback/{provider.Id}";

        // Store state
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(OidcCallback), new { providerId = provider.Id }),
            Items =
            {
                { "returnUrl", returnUrl ?? "/" },
                { "providerId", provider.Id.ToString() },
                { "scheme", "oidc" }
            }
        };

        // Configure and challenge
        // For dynamic OIDC, we'll use a custom approach
        var oidcOptions = BuildOidcOptions(provider);

        // Store options in TempData for the callback
        TempData["OidcProviderId"] = provider.Id;
        TempData["OidcReturnUrl"] = returnUrl;

        // Redirect to authorization endpoint
        var authorizationUrl = BuildOidcAuthorizationUrl(provider, callbackPath, properties);
        return Redirect(authorizationUrl);
    }

    private string BuildOidcAuthorizationUrl(CompanyIdentityProvider provider, string callbackPath, AuthenticationProperties properties)
    {
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var redirectUri = $"{baseUrl}/Account/OidcCallback?providerId={provider.Id}";

        // Build state
        var state = Guid.NewGuid().ToString("N");
        TempData[$"oidc_state_{state}"] = System.Text.Json.JsonSerializer.Serialize(properties.Items);

        // Get scopes
        var scopes = provider.OidcScopes?.Split(',') ?? new[] { "openid", "profile", "email" };
        var scopeString = string.Join(" ", scopes);

        // Build authorization URL
        var authEndpoint = $"{provider.OidcAuthority?.TrimEnd('/')}/authorize";

        // Handle common authority formats
        if (provider.OidcAuthority?.Contains("login.microsoftonline.com") == true)
        {
            authEndpoint = $"{provider.OidcAuthority?.TrimEnd('/')}/oauth2/v2.0/authorize";
        }

        var queryParams = new Dictionary<string, string>
        {
            { "client_id", provider.OidcClientId! },
            { "response_type", provider.OidcResponseType ?? "code" },
            { "redirect_uri", redirectUri },
            { "scope", scopeString },
            { "state", state },
            { "response_mode", "query" }
        };

        if (provider.OidcUsePkce)
        {
            // Generate PKCE challenge
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            TempData[$"oidc_verifier_{state}"] = codeVerifier;
            queryParams["code_challenge"] = codeChallenge;
            queryParams["code_challenge_method"] = "S256";
        }

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{authEndpoint}?{queryString}";
    }

    [HttpGet]
    public async Task<IActionResult> OidcCallback(int providerId, string? code = null, string? state = null, string? error = null, string? error_description = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("OIDC callback error for provider {ProviderId}: {Error} - {Description}",
                providerId, error, error_description);
            return RedirectToAction(nameof(Login), new { error = error_description ?? error });
        }

        if (string.IsNullOrEmpty(code))
        {
            return RedirectToAction(nameof(Login), new { error = "No authorization code received" });
        }

        var provider = await _authConfigService.GetIdentityProviderByIdAsync(providerId);
        if (provider == null)
        {
            return RedirectToAction(nameof(Login), new { error = "Provider not found" });
        }

        try
        {
            // Get stored state
            var stateJson = TempData[$"oidc_state_{state}"]?.ToString();
            var stateItems = string.IsNullOrEmpty(stateJson)
                ? new Dictionary<string, string?>()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string?>>(stateJson)!;

            var returnUrl = stateItems.GetValueOrDefault("returnUrl") ?? "/";
            var codeVerifier = TempData[$"oidc_verifier_{state}"]?.ToString();

            // Exchange code for tokens
            var principal = await ExchangeOidcCode(provider, code, codeVerifier);

            if (principal == null)
            {
                return RedirectToAction(nameof(Login), new { error = "Failed to authenticate" });
            }

            // Map external user to local user
            var mappingResult = await _externalUserMapper.MapExternalUserAsync(principal, provider, provider.CompanyId);

            if (!mappingResult.Succeeded)
            {
                return RedirectToAction(nameof(Login), new { error = mappingResult.ErrorMessage });
            }

            // Sign in the user
            await _signInManager.SignInAsync(mappingResult.User!, isPersistent: false);

            _logger.LogInformation("User {UserId} logged in via OIDC provider {Provider}",
                mappingResult.User!.Id, provider.Name);

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OIDC callback for provider {ProviderId}", providerId);
            return RedirectToAction(nameof(Login), new { error = "Authentication failed" });
        }
    }

    private async Task<ClaimsPrincipal?> ExchangeOidcCode(CompanyIdentityProvider provider, string code, string? codeVerifier)
    {
        using var httpClient = new HttpClient();

        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var redirectUri = $"{baseUrl}/Account/OidcCallback?providerId={provider.Id}";

        // Build token endpoint
        var tokenEndpoint = $"{provider.OidcAuthority?.TrimEnd('/')}/token";
        if (provider.OidcAuthority?.Contains("login.microsoftonline.com") == true)
        {
            tokenEndpoint = $"{provider.OidcAuthority?.TrimEnd('/')}/oauth2/v2.0/token";
        }

        var tokenParams = new Dictionary<string, string>
        {
            { "client_id", provider.OidcClientId! },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        // Add client secret if configured
        if (!string.IsNullOrEmpty(provider.OidcClientSecretEncrypted))
        {
            var clientSecret = _encryptionService.DecryptIfNotEmpty(provider.OidcClientSecretEncrypted);
            if (!string.IsNullOrEmpty(clientSecret))
            {
                tokenParams["client_secret"] = clientSecret;
            }
        }

        // Add PKCE verifier if used
        if (!string.IsNullOrEmpty(codeVerifier))
        {
            tokenParams["code_verifier"] = codeVerifier;
        }

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenParams));
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Token exchange failed: {Response}", responseContent);
            return null;
        }

        var tokenResponse = System.Text.Json.JsonDocument.Parse(responseContent);
        var idToken = tokenResponse.RootElement.GetProperty("id_token").GetString();

        if (string.IsNullOrEmpty(idToken))
        {
            _logger.LogError("No id_token in response");
            return null;
        }

        // Parse JWT (simplified - in production, validate signature)
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(idToken);

        var identity = new ClaimsIdentity(token.Claims, "oidc");
        return new ClaimsPrincipal(identity);
    }

    private IActionResult ChallengeSaml(CompanyIdentityProvider provider, string? returnUrl)
    {
        // SAML implementation would go here
        // For now, return not implemented
        return RedirectToAction(nameof(Login), new { error = "SAML authentication is not yet implemented" });
    }

    private IActionResult ChallengeWsFed(CompanyIdentityProvider provider, string? returnUrl)
    {
        // WS-Federation implementation would go here
        // For now, return not implemented
        return RedirectToAction(nameof(Login), new { error = "WS-Federation authentication is not yet implemented" });
    }

    private OpenIdConnectOptions BuildOidcOptions(CompanyIdentityProvider provider)
    {
        var options = new OpenIdConnectOptions
        {
            Authority = provider.OidcAuthority,
            ClientId = provider.OidcClientId,
            ResponseType = provider.OidcResponseType ?? OpenIdConnectResponseType.Code,
            SaveTokens = true,
            GetClaimsFromUserInfoEndpoint = provider.OidcGetClaimsFromUserInfoEndpoint,
            RequireHttpsMetadata = provider.OidcRequireHttpsMetadata,
            UsePkce = provider.OidcUsePkce
        };

        // Add scopes
        var scopes = provider.OidcScopes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ??
                     new[] { "openid", "profile", "email" };
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope.Trim());
        }

        // Add client secret if available
        if (!string.IsNullOrEmpty(provider.OidcClientSecretEncrypted))
        {
            options.ClientSecret = _encryptionService.DecryptIfNotEmpty(provider.OidcClientSecretEncrypted);
        }

        return options;
    }

    [HttpGet]
    public IActionResult LoginWith2fa(string? returnUrl = null, bool rememberMe = false)
    {
        return View(new LoginWith2faViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _companyContext.ClearCurrentCompany();
        _logger.LogInformation("User logged out");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.ASCII.GetBytes(codeVerifier);
        var hash = sha256.ComputeHash(bytes);
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
