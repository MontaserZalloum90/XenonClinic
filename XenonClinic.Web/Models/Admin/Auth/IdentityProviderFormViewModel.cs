using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Entities;

namespace XenonClinic.Web.Models.Admin.Auth;

public class IdentityProviderFormViewModel
{
    public int? Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Internal Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "Provider Type")]
    public IdentityProviderType Type { get; set; }

    [Display(Name = "Is Default Provider")]
    public bool IsDefault { get; set; }

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Icon CSS Class")]
    public string? IconClass { get; set; }

    [Display(Name = "Button CSS Class")]
    public string? ButtonClass { get; set; }

    // ========================================
    // OIDC Configuration
    // ========================================

    [Display(Name = "Authority URL")]
    public string? OidcAuthority { get; set; }

    [Display(Name = "Client ID")]
    public string? OidcClientId { get; set; }

    [Display(Name = "Client Secret")]
    [DataType(DataType.Password)]
    public string? OidcClientSecret { get; set; }

    [Display(Name = "Callback Path")]
    public string? OidcCallbackPath { get; set; }

    [Display(Name = "Signed Out Callback Path")]
    public string? OidcSignedOutCallbackPath { get; set; }

    [Display(Name = "Response Type")]
    public string? OidcResponseType { get; set; }

    [Display(Name = "Use PKCE")]
    public bool OidcUsePkce { get; set; } = true;

    [Display(Name = "Scopes (comma-separated)")]
    public string? OidcScopes { get; set; }

    [Display(Name = "Require HTTPS Metadata")]
    public bool OidcRequireHttpsMetadata { get; set; } = true;

    [Display(Name = "Get Claims from UserInfo Endpoint")]
    public bool OidcGetClaimsFromUserInfoEndpoint { get; set; } = true;

    // ========================================
    // SAML 2.0 Configuration
    // ========================================

    [Display(Name = "SP Entity ID")]
    public string? SamlEntityId { get; set; }

    [Display(Name = "ACS Path")]
    public string? SamlAcsPath { get; set; }

    [Display(Name = "SLO Path")]
    public string? SamlSloPath { get; set; }

    [Display(Name = "IdP Metadata URL")]
    public string? SamlMetadataUrl { get; set; }

    [Display(Name = "IdP Entity ID")]
    public string? SamlIdpEntityId { get; set; }

    [Display(Name = "Sign Authentication Requests")]
    public bool SamlSignAuthnRequests { get; set; }

    [Display(Name = "SP Certificate (base64)")]
    public string? SamlSpCertificate { get; set; }

    [Display(Name = "SP Certificate Password")]
    [DataType(DataType.Password)]
    public string? SamlSpCertificatePassword { get; set; }

    [Display(Name = "Want Assertions Signed")]
    public bool SamlWantAssertionsSigned { get; set; } = true;

    [Display(Name = "Allowed Clock Skew (minutes)")]
    public int SamlAllowedClockSkewMinutes { get; set; } = 5;

    // ========================================
    // WS-Federation Configuration
    // ========================================

    [Display(Name = "Metadata Address")]
    public string? WsFedMetadataAddress { get; set; }

    [Display(Name = "Wtrealm")]
    public string? WsFedWtrealm { get; set; }

    [Display(Name = "Reply URL")]
    public string? WsFedReplyUrl { get; set; }

    [Display(Name = "Require HTTPS")]
    public bool WsFedRequireHttps { get; set; } = true;

    // ========================================
    // Claim Mappings
    // ========================================

    [Display(Name = "Email Claim Type")]
    public string? ClaimMappingEmail { get; set; }

    [Display(Name = "UPN Claim Type")]
    public string? ClaimMappingUpn { get; set; }

    [Display(Name = "Name Claim Type")]
    public string? ClaimMappingName { get; set; }

    [Display(Name = "First Name Claim Type")]
    public string? ClaimMappingFirstName { get; set; }

    [Display(Name = "Last Name Claim Type")]
    public string? ClaimMappingLastName { get; set; }

    [Display(Name = "Groups Claim Type")]
    public string? ClaimMappingGroups { get; set; }

    [Display(Name = "Extra Configuration (JSON)")]
    public string? ExtraConfigJson { get; set; }

    public bool IsEdit => Id.HasValue && Id.Value > 0;
}
