using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an insurance plan offered by a provider
/// </summary>
public class InsurancePlan : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Reference to the insurance provider
    /// </summary>
    public int ProviderId { get; set; }
    public InsuranceProvider Provider { get; set; } = null!;

    /// <summary>
    /// Plan code (unique within provider)
    /// </summary>
    public string PlanCode { get; set; } = string.Empty;

    /// <summary>
    /// Plan name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Name in Arabic
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// Plan description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Plan type (Basic, Standard, Premium, VIP, etc.)
    /// </summary>
    public string PlanType { get; set; } = "Standard";

    /// <summary>
    /// Network type (In-Network, Out-of-Network, PPO, HMO)
    /// </summary>
    public string NetworkType { get; set; } = "In-Network";

    /// <summary>
    /// Whether plan is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Coverage percentage (0-100)
    /// </summary>
    public decimal CoveragePercent { get; set; } = 80;

    /// <summary>
    /// Patient copay percentage
    /// </summary>
    public decimal CopayPercent { get; set; } = 20;

    /// <summary>
    /// Fixed copay amount (if applicable)
    /// </summary>
    public decimal? CopayAmount { get; set; }

    /// <summary>
    /// Deductible amount
    /// </summary>
    public decimal? DeductibleAmount { get; set; }

    /// <summary>
    /// Annual maximum coverage
    /// </summary>
    public decimal? AnnualMaximum { get; set; }

    /// <summary>
    /// Lifetime maximum coverage
    /// </summary>
    public decimal? LifetimeMaximum { get; set; }

    /// <summary>
    /// Maximum per visit
    /// </summary>
    public decimal? MaxPerVisit { get; set; }

    /// <summary>
    /// Pre-authorization required
    /// </summary>
    public bool RequiresPreAuth { get; set; }

    /// <summary>
    /// Referral required
    /// </summary>
    public bool RequiresReferral { get; set; }

    /// <summary>
    /// Covered services (JSON array of service codes)
    /// </summary>
    public string? CoveredServices { get; set; }

    /// <summary>
    /// Excluded services (JSON array of service codes)
    /// </summary>
    public string? ExcludedServices { get; set; }

    /// <summary>
    /// Waiting period in days for new members
    /// </summary>
    public int WaitingPeriodDays { get; set; }

    /// <summary>
    /// Grace period in days for claims submission
    /// </summary>
    public int ClaimGracePeriodDays { get; set; } = 30;

    /// <summary>
    /// Notes about this plan
    /// </summary>
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
