namespace XenonClinic.Core.Entities;

/// <summary>
/// Type of identity provider
/// </summary>
public enum IdentityProviderType
{
    /// <summary>OpenID Connect provider (e.g., Azure AD, ADFS with OIDC)</summary>
    OIDC = 0,
    /// <summary>SAML 2.0 provider (e.g., ADFS, Okta, OneLogin)</summary>
    SAML2 = 1,
    /// <summary>WS-Federation provider (e.g., legacy ADFS)</summary>
    WSFED = 2
}
