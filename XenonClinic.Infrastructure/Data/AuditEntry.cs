namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Represents an audit entry for tracking changes to entities
/// </summary>
public class AuditEntry
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public AuditAction Action { get; set; }
    public Dictionary<string, object?> OldValues { get; set; } = new();
    public Dictionary<string, object?> NewValues { get; set; } = new();
    public List<string> ChangedProperties { get; set; } = new();
}

/// <summary>
/// Audit action types
/// </summary>
public enum AuditAction
{
    Create,
    Update,
    Delete,
    SoftDelete,
    Restore
}
