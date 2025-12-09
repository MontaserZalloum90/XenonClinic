namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a clinic tenant instance - the top level of the multi-tenancy hierarchy.
/// A tenant is a group of companies within a single clinic deployment.
/// Links to Xenon.Platform's Tenant entity via PlatformTenantId for SaaS management.
/// </summary>
public class Tenant
{
    public int Id { get; set; }

    /// <summary>
    /// Link to the Platform's Tenant entity (Guid-based).
    /// This connects the clinic instance to the SaaS management platform.
    /// </summary>
    public Guid? PlatformTenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // License/Subscription information (synced from Platform)
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    public int MaxCompanies { get; set; } = 5;
    public int MaxBranchesPerCompany { get; set; } = 10;
    public int MaxUsersPerTenant { get; set; } = 100;

    // Navigation properties
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public TenantSettings? Settings { get; set; }

    // Configuration overrides
    public ICollection<TenantFeature> Features { get; set; } = new List<TenantFeature>();
    public ICollection<TenantTerminology> Terminology { get; set; } = new List<TenantTerminology>();
    public ICollection<TenantUISchema> UISchemas { get; set; } = new List<TenantUISchema>();
    public ICollection<TenantFormLayout> FormLayouts { get; set; } = new List<TenantFormLayout>();
    public ICollection<TenantListLayout> ListLayouts { get; set; } = new List<TenantListLayout>();
    public TenantNavigation? Navigation { get; set; }
}
