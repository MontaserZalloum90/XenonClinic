using Microsoft.AspNetCore.Identity;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Entities;

/// <summary>
/// Application user entity - concrete implementation using ASP.NET Identity.
/// This is in Infrastructure because it depends on Microsoft.AspNetCore.Identity.
/// </summary>
public class ApplicationUser : IdentityUser, IApplicationUser
{
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public int? PrimaryBranchId { get; set; }

    /// <summary>
    /// The default branch for this user when no specific branch is selected.
    /// Falls back to PrimaryBranchId if not set.
    /// </summary>
    public int? DefaultBranchId { get; set; }

    /// <summary>
    /// Whether this user is a company administrator with access to all branches
    /// within their company. Only applies to non-super-admin users.
    /// </summary>
    public bool IsCompanyAdmin { get; set; } = false;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarPath { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
    public string? Timezone { get; set; } = "Arabian Standard Time";
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // External login tracking
    /// <summary>
    /// Whether this user was auto-provisioned from an external IdP
    /// </summary>
    public bool IsExternalUser { get; set; } = false;

    /// <summary>
    /// The identity provider that provisioned this user (e.g., "ADFS-OIDC")
    /// </summary>
    public string? ExternalProviderName { get; set; }

    /// <summary>
    /// The external identifier from the IdP (usually sub or nameidentifier claim)
    /// </summary>
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// Last external login timestamp
    /// </summary>
    public DateTime? LastExternalLoginAt { get; set; }

    // Navigation properties - these reference Core entities
    // Note: These are configured in DbContext, not using attributes to avoid circular references
}
