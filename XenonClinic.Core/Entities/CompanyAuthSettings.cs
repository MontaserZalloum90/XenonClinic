namespace XenonClinic.Core.Entities;

/// <summary>
/// Per-company authentication settings.
/// </summary>
public class CompanyAuthSettings
{
    public int Id { get; set; }
    public int CompanyId { get; set; }

    /// <summary>
    /// The authentication mode for this company
    /// </summary>
    public AuthMode AuthMode { get; set; } = AuthMode.LocalOnly;

    /// <summary>
    /// Whether local username/password login is allowed
    /// </summary>
    public bool AllowLocalLogin { get; set; } = true;

    /// <summary>
    /// Whether external SSO login is allowed
    /// </summary>
    public bool AllowExternalLogin { get; set; } = false;

    /// <summary>
    /// Whether to automatically create users on first SSO login
    /// </summary>
    public bool AutoProvisionUsers { get; set; } = false;

    /// <summary>
    /// Default role to assign to auto-provisioned users
    /// </summary>
    public string? DefaultRoleOnAutoProvision { get; set; } = "User";

    /// <summary>
    /// Name of the default external provider (if multiple configured)
    /// </summary>
    public string? DefaultExternalProviderName { get; set; }

    /// <summary>
    /// Whether this configuration is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Custom login page message/instructions
    /// </summary>
    public string? LoginPageMessage { get; set; }

    /// <summary>
    /// Custom login page message/instructions in Arabic
    /// </summary>
    public string? LoginPageMessageAr { get; set; }

    /// <summary>
    /// URL to redirect after successful login
    /// </summary>
    public string? PostLoginRedirectUrl { get; set; }

    /// <summary>
    /// URL to redirect after logout
    /// </summary>
    public string? PostLogoutRedirectUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<CompanyIdentityProvider> IdentityProviders { get; set; } = new List<CompanyIdentityProvider>();
}
