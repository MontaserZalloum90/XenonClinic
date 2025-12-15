namespace XenonClinic.Core.Entities;

/// <summary>
/// Configuration for an external identity provider for a company.
/// </summary>
public class CompanyIdentityProvider
{
    public int Id { get; set; }
    public int CompanyId { get; set; }

    /// <summary>
    /// Internal unique name for this provider (e.g., "ADFS-OIDC", "Okta-SAML")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown on login button (e.g., "Sign in with Corporate SSO")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the identity provider
    /// </summary>
    public IdentityProviderType Type { get; set; }

    /// <summary>
    /// Whether this is the default provider for the company
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether this provider is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Sort order for display on login page
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Icon class for the login button (e.g., "bi bi-microsoft" for Bootstrap Icons)
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Button color class (e.g., "btn-primary", "btn-outline-dark")
    /// </summary>
    public string? ButtonClass { get; set; } = "btn-outline-primary";

    // ========================================
    // OIDC Configuration
    // ========================================

    /// <summary>OIDC: Authority URL (e.g., https://login.microsoftonline.com/{tenant}/v2.0)</summary>
    public string? OidcAuthority { get; set; }

    /// <summary>OIDC: Client ID (Application ID)</summary>
    public string? OidcClientId { get; set; }

    /// <summary>OIDC: Client Secret (encrypted)</summary>
    public string? OidcClientSecretEncrypted { get; set; }

    /// <summary>OIDC: Callback path for authentication response</summary>
    public string? OidcCallbackPath { get; set; } = "/signin-oidc";

    /// <summary>OIDC: Signed out callback path</summary>
    public string? OidcSignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    /// <summary>OIDC: Response type (default: "code")</summary>
    public string? OidcResponseType { get; set; } = "code";

    /// <summary>OIDC: Whether to use PKCE</summary>
    public bool OidcUsePkce { get; set; } = true;

    /// <summary>OIDC: Scopes (comma-separated, e.g., "openid,profile,email")</summary>
    public string? OidcScopes { get; set; } = "openid,profile,email";

    /// <summary>OIDC: Whether to require HTTPS for metadata</summary>
    public bool OidcRequireHttpsMetadata { get; set; } = true;

    /// <summary>OIDC: Get back channel logout session requirement</summary>
    public bool OidcGetClaimsFromUserInfoEndpoint { get; set; } = true;

    // ========================================
    // SAML 2.0 Configuration
    // ========================================

    /// <summary>SAML: Service Provider Entity ID</summary>
    public string? SamlEntityId { get; set; }

    /// <summary>SAML: Assertion Consumer Service path</summary>
    public string? SamlAcsPath { get; set; } = "/saml/acs";

    /// <summary>SAML: Single Logout Service path</summary>
    public string? SamlSloPath { get; set; } = "/saml/slo";

    /// <summary>SAML: Identity Provider Metadata URL</summary>
    public string? SamlMetadataUrl { get; set; }

    /// <summary>SAML: Identity Provider Entity ID</summary>
    public string? SamlIdpEntityId { get; set; }

    /// <summary>SAML: Whether to sign authentication requests</summary>
    public bool SamlSignAuthnRequests { get; set; } = false;

    /// <summary>SAML: SP Certificate path or base64 encoded certificate</summary>
    public string? SamlSpCertificate { get; set; }

    /// <summary>SAML: SP Certificate password (encrypted)</summary>
    public string? SamlSpCertificatePasswordEncrypted { get; set; }

    /// <summary>SAML: Want assertions signed</summary>
    public bool SamlWantAssertionsSigned { get; set; } = true;

    /// <summary>SAML: Allowed clock skew in minutes</summary>
    public int SamlAllowedClockSkewMinutes { get; set; } = 5;

    // ========================================
    // WS-Federation Configuration
    // ========================================

    /// <summary>WS-Fed: Metadata address</summary>
    public string? WsFedMetadataAddress { get; set; }

    /// <summary>WS-Fed: Wtrealm (Relying Party identifier)</summary>
    public string? WsFedWtrealm { get; set; }

    /// <summary>WS-Fed: Reply URL (wreply)</summary>
    public string? WsFedReplyUrl { get; set; }

    /// <summary>WS-Fed: Require HTTPS for metadata</summary>
    public bool WsFedRequireHttps { get; set; } = true;

    // ========================================
    // Claim Mappings
    // ========================================

    /// <summary>Claim type for email address</summary>
    public string? ClaimMappingEmail { get; set; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

    /// <summary>Claim type for User Principal Name (UPN)</summary>
    public string? ClaimMappingUpn { get; set; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

    /// <summary>Claim type for display name</summary>
    public string? ClaimMappingName { get; set; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    /// <summary>Claim type for first name</summary>
    public string? ClaimMappingFirstName { get; set; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";

    /// <summary>Claim type for last name</summary>
    public string? ClaimMappingLastName { get; set; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

    /// <summary>Claim type for groups/roles</summary>
    public string? ClaimMappingGroups { get; set; } = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups";

    /// <summary>
    /// JSON for any provider-specific advanced configuration
    /// </summary>
    public string? ExtraConfigJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
}
