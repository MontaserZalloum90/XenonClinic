namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a company within a tenant.
/// A company can have multiple branches.
/// </summary>
public class Company
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TradeLicenseNumber { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "UAE";
    public string? PostalCode { get; set; }
    public string? LogoPath { get; set; }
    public string? Website { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public string? Currency { get; set; } = "AED";
    public string? Timezone { get; set; } = "Arabian Standard Time";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    /// <summary>
    /// Authentication settings for this company
    /// </summary>
    public CompanyAuthSettings? AuthSettings { get; set; }

    /// <summary>
    /// Company-specific configuration settings (can override tenant settings)
    /// </summary>
    public CompanySettings? Settings { get; set; }
}
