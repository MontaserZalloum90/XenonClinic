namespace XenonClinic.Core.Entities;

/// <summary>
/// Audit log entity for tracking all system changes for compliance
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    /// <summary>
    /// Unique correlation ID to track requests across the system
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The type of action performed (Create, Update, Delete, Read, Login, Logout, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The entity type being acted upon (Patient, Appointment, etc.)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// The ID of the entity being acted upon
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username for display purposes
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// The HTTP method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// The request path/endpoint
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP status code of the response
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// JSON serialized old values (for updates)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON serialized new values (for creates and updates)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Additional context or description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// Company ID for multi-tenancy
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Branch ID for multi-tenancy
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// When the action was performed (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the request in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Module name (CaseManagement, Audiology, etc.)
    /// </summary>
    public string? ModuleName { get; set; }
}

/// <summary>
/// Enum for common audit actions
/// </summary>
public static class AuditActions
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string LoginFailed = "LoginFailed";
    public const string PasswordChange = "PasswordChange";
    public const string PasswordReset = "PasswordReset";
    public const string Export = "Export";
    public const string Import = "Import";
    public const string Access = "Access";
    public const string AccessDenied = "AccessDenied";
    public const string RateLimited = "RateLimited";
}
