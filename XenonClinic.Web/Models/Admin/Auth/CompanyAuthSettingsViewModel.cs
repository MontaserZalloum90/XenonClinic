using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Entities;

namespace XenonClinic.Web.Models.Admin.Auth;

public class CompanyAuthSettingsViewModel
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;

    // Auth Mode
    [Display(Name = "Authentication Mode")]
    public AuthMode AuthMode { get; set; } = AuthMode.LocalOnly;

    [Display(Name = "Allow Local Login")]
    public bool AllowLocalLogin { get; set; } = true;

    [Display(Name = "Allow External Login (SSO)")]
    public bool AllowExternalLogin { get; set; }

    [Display(Name = "Auto-Provision External Users")]
    public bool AutoProvisionUsers { get; set; }

    [Display(Name = "Default Role for Auto-Provisioned Users")]
    public string? DefaultRoleOnAutoProvision { get; set; } = "User";

    [Display(Name = "Default External Provider")]
    public string? DefaultExternalProviderName { get; set; }

    [Display(Name = "Enable Custom Auth Settings")]
    public bool IsEnabled { get; set; }

    [Display(Name = "Login Page Message (English)")]
    public string? LoginPageMessage { get; set; }

    [Display(Name = "Login Page Message (Arabic)")]
    public string? LoginPageMessageAr { get; set; }

    [Display(Name = "Post-Login Redirect URL")]
    public string? PostLoginRedirectUrl { get; set; }

    [Display(Name = "Post-Logout Redirect URL")]
    public string? PostLogoutRedirectUrl { get; set; }

    // Identity Providers
    public IList<IdentityProviderItemViewModel> IdentityProviders { get; set; } = new List<IdentityProviderItemViewModel>();

    // Available options
    public List<string> AvailableRoles { get; set; } = new();
}

public class IdentityProviderItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IdentityProviderType Type { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }

    public string TypeDisplayName => Type switch
    {
        IdentityProviderType.OIDC => "OpenID Connect",
        IdentityProviderType.SAML2 => "SAML 2.0",
        IdentityProviderType.WSFED => "WS-Federation",
        _ => Type.ToString()
    };
}
