namespace Xenon.Platform.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON

    public string? PerformedBy { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? PerformedByRole { get; set; }

    public Guid? TenantId { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
