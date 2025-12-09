using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LegalName { get; set; }

    public CompanyType CompanyType { get; set; }
    public ClinicType? ClinicType { get; set; }

    public TenantStatus Status { get; set; } = TenantStatus.Trial;

    public DateTime TrialStartDate { get; set; }
    public DateTime TrialEndDate { get; set; }

    // Contact Info
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }

    // Database Connection
    public string? DatabaseConnectionString { get; set; }
    public bool IsDatabaseProvisioned { get; set; } = false;
    public DateTime? DatabaseProvisionedAt { get; set; }

    // License Guardrails (from subscription)
    public int MaxBranches { get; set; } = 1;
    public int MaxUsers { get; set; } = 5;

    // Current Usage (updated by metering service)
    public int CurrentBranches { get; set; } = 0;
    public int CurrentUsers { get; set; } = 0;

    // Relationships
    public ICollection<TenantAdmin> Admins { get; set; } = new List<TenantAdmin>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<UsageSnapshot> UsageSnapshots { get; set; } = new List<UsageSnapshot>();
    public ICollection<TenantHealthCheck> HealthChecks { get; set; } = new List<TenantHealthCheck>();

    // Computed
    public bool IsTrialExpired => Status == TenantStatus.Trial && DateTime.UtcNow > TrialEndDate;
    public int TrialDaysRemaining => Status == TenantStatus.Trial
        ? Math.Max(0, (TrialEndDate - DateTime.UtcNow).Days)
        : 0;
    public Subscription? ActiveSubscription => Subscriptions
        .FirstOrDefault(s => s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.UtcNow);
}
