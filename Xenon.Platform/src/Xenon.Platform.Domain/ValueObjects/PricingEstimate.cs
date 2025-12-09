using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.ValueObjects;

public record PricingEstimate
{
    public PlanCode PlanCode { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public BillingCycle BillingCycle { get; init; }
    public Currency Currency { get; init; }

    public int Branches { get; init; }
    public int Users { get; init; }

    public int IncludedBranches { get; init; }
    public int IncludedUsers { get; init; }

    public int ExtraBranches { get; init; }
    public int ExtraUsers { get; init; }

    public decimal BasePrice { get; init; }
    public decimal ExtraBranchesPrice { get; init; }
    public decimal ExtraUsersPrice { get; init; }

    public decimal Subtotal { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal Total { get; init; }

    public decimal MonthlyEquivalent { get; init; }

    public IReadOnlyList<PricingLineItem> Breakdown { get; init; } = Array.Empty<PricingLineItem>();
}

public record PricingLineItem
{
    public string Label { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public bool IsDiscount { get; init; }
}
