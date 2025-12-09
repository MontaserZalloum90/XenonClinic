namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Audit trail service for tracking entity changes.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log an audit entry.
    /// </summary>
    Task LogAsync(AuditEntry entry);

    /// <summary>
    /// Get audit history for an entity.
    /// </summary>
    Task<IEnumerable<AuditEntry>> GetHistoryAsync(string entityType, string entityId);

    /// <summary>
    /// Get audit entries by user.
    /// </summary>
    Task<IEnumerable<AuditEntry>> GetByUserAsync(string userId, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Get audit entries by action type.
    /// </summary>
    Task<IEnumerable<AuditEntry>> GetByActionAsync(AuditAction action, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Search audit entries.
    /// </summary>
    Task<PagedResult<AuditEntry>> SearchAsync(AuditSearchCriteria criteria);
}

/// <summary>
/// Audit entry record.
/// </summary>
public class AuditEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object?> OldValues { get; set; } = new();
    public Dictionary<string, object?> NewValues { get; set; } = new();
    public List<string> ChangedProperties { get; set; } = new();
    public string? AdditionalData { get; set; }
}

/// <summary>
/// Audit action types.
/// </summary>
public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    SoftDelete = 4,
    Restore = 5,
    View = 6,
    Export = 7,
    Login = 8,
    Logout = 9,
    FailedLogin = 10
}

/// <summary>
/// Audit search criteria.
/// </summary>
public class AuditSearchCriteria
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchText { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Paged result for queries.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
