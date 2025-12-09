using Xenon.Platform.Domain.Enums;

namespace Xenon.Platform.Domain.Entities;

public class TenantHealthCheck : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public DateTime CheckedAt { get; set; }

    public HealthStatus DatabaseStatus { get; set; }
    public int? DatabaseLatencyMs { get; set; }
    public string? DatabaseError { get; set; }

    public HealthStatus OverallStatus { get; set; }

    public string? Details { get; set; } // JSON with additional health info
}
