using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class SubscriptionHistory : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;

    public string Action { get; set; } = string.Empty; // Created, Renewed, Upgraded, Downgraded, Cancelled, Expired

    public PlanCode? PreviousPlan { get; set; }
    public PlanCode? NewPlan { get; set; }

    public SubscriptionStatus? PreviousStatus { get; set; }
    public SubscriptionStatus? NewStatus { get; set; }

    public string? Notes { get; set; }
    public string? PerformedBy { get; set; } // Admin email or "System"
}
