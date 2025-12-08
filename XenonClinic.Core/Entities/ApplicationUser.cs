using Microsoft.AspNetCore.Identity;

namespace XenonClinic.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public int? PrimaryBranchId { get; set; }

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

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public Company? Company { get; set; }
    public Branch? PrimaryBranch { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
