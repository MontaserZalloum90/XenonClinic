namespace Xenon.Platform.Domain.Entities;

public class UsageSnapshot : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public DateTime SnapshotDate { get; set; }
    public string SnapshotType { get; set; } = "Daily"; // Hourly, Daily, Monthly

    // User metrics
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
    public int NewUsersCount { get; set; }

    // Branch metrics
    public int ActiveBranches { get; set; }

    // API Usage
    public long ApiCallsCount { get; set; }
    public long ApiErrorsCount { get; set; }

    // Storage (placeholder for future)
    public long StorageUsedBytes { get; set; }
    public int DocumentsCount { get; set; }

    // Records
    public int PatientsCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int InvoicesCount { get; set; }

    // Session metrics
    public int TotalSessions { get; set; }
    public double AvgSessionDurationMinutes { get; set; }
}
