using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a marketing campaign for clinic outreach and patient engagement
/// </summary>
public class Campaign
{
    public int Id { get; set; }

    /// <summary>
    /// Unique campaign reference code (e.g., CAMP-2024-001)
    /// </summary>
    public string CampaignCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CampaignType Type { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    // Campaign period
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public decimal? ActualSpend { get; set; }

    // Target metrics
    public int? TargetLeads { get; set; }
    public int? TargetConversions { get; set; }

    // Actual metrics
    public int LeadsGenerated { get; set; }
    public int Conversions { get; set; }
    public decimal? Revenue { get; set; }

    // Target audience
    public string? TargetAudience { get; set; }
    public string? Tags { get; set; }

    // Content
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public string? CallToAction { get; set; }

    // Branch
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    // Assignment
    public string? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }

    // Audit
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation Properties
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<MarketingActivity> Activities { get; set; } = new List<MarketingActivity>();

    // Computed Properties
    public decimal? ROI => ActualSpend > 0 && Revenue > 0
        ? ((Revenue - ActualSpend) / ActualSpend) * 100
        : null;

    public decimal? ConversionRate => LeadsGenerated > 0
        ? ((decimal)Conversions / LeadsGenerated) * 100
        : null;

    public decimal? CostPerLead => LeadsGenerated > 0 && ActualSpend > 0
        ? ActualSpend / LeadsGenerated
        : null;

    public bool IsActive => Status == CampaignStatus.Active
        && StartDate <= DateTime.UtcNow
        && (!EndDate.HasValue || EndDate.Value >= DateTime.UtcNow);
}
