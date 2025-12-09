using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    public PlanCode PlanCode { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public BillingCycle BillingCycle { get; set; }
    public Currency Currency { get; set; } = Currency.AED;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Purchased resources
    public int BranchesPurchased { get; set; }
    public int UsersPurchased { get; set; }

    // Pricing snapshot at time of purchase
    public decimal BasePrice { get; set; }
    public decimal ExtraBranchesPrice { get; set; }
    public decimal ExtraUsersPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }

    // Stripe integration
    public string? StripeSubscriptionId { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    // Auto-renewal
    public bool AutoRenew { get; set; } = true;
    public DateTime? NextBillingDate { get; set; }

    // Cancellation
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<SubscriptionHistory> History { get; set; } = new List<SubscriptionHistory>();

    public bool IsExpired => EndDate < DateTime.UtcNow;
    public int DaysRemaining => Math.Max(0, (EndDate - DateTime.UtcNow).Days);
}
