using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class Plan : BaseEntity
{
    public PlanCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Pricing in AED
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }

    // Included Resources
    public int IncludedBranches { get; set; }
    public int IncludedUsers { get; set; }

    // Extra Resource Pricing
    public decimal ExtraBranchPrice { get; set; }
    public decimal ExtraUserPrice { get; set; }

    // Features (JSON array)
    public string FeaturesJson { get; set; } = "[]";

    public string SupportLevel { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsRecommended { get; set; } = false;

    public int SortOrder { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
