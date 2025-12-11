namespace Xenon.Platform.Domain.Entities;

/// <summary>
/// Tracks security-related events for monitoring and compliance.
/// </summary>
public class SecurityEvent : BaseEntity
{
    /// <summary>
    /// Type of security event
    /// </summary>
    public SecurityEventType EventType { get; set; }

    /// <summary>
    /// User ID associated with the event (if applicable)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// User type: "PlatformAdmin" or "TenantAdmin"
    /// </summary>
    public string? UserType { get; set; }

    /// <summary>
    /// Tenant ID (for tenant-related events)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Email address involved in the event
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Geographic location (if resolved from IP)
    /// </summary>
    public string? GeoLocation { get; set; }

    /// <summary>
    /// Device fingerprint
    /// </summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// Whether the event was successful
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Risk level assessed for this event
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Additional details about the event
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error message if the event failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Session ID if applicable
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Request ID for correlation
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Whether the event triggered an alert
    /// </summary>
    public bool AlertTriggered { get; set; }

    /// <summary>
    /// Whether the event has been reviewed by security team
    /// </summary>
    public bool IsReviewed { get; set; }

    /// <summary>
    /// Who reviewed the event
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// When the event was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Notes from the security review
    /// </summary>
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// Types of security events to track
/// </summary>
public enum SecurityEventType
{
    // Authentication events
    LoginSuccess = 1,
    LoginFailed = 2,
    LoginLockedOut = 3,
    LogoutSuccess = 4,
    TokenRefresh = 5,
    TokenRevoked = 6,

    // Password events
    PasswordChanged = 10,
    PasswordResetRequested = 11,
    PasswordResetCompleted = 12,
    PasswordResetFailed = 13,

    // Account events
    AccountCreated = 20,
    AccountActivated = 21,
    AccountDeactivated = 22,
    AccountDeleted = 23,
    AccountLocked = 24,
    AccountUnlocked = 25,

    // MFA events
    MfaEnabled = 30,
    MfaDisabled = 31,
    MfaVerified = 32,
    MfaFailed = 33,
    MfaBackupUsed = 34,

    // Session events
    SessionCreated = 40,
    SessionExpired = 41,
    SessionTerminated = 42,
    ConcurrentSessionBlocked = 43,

    // Permission events
    PermissionGranted = 50,
    PermissionRevoked = 51,
    RoleChanged = 52,

    // Suspicious activity
    SuspiciousIpAddress = 60,
    BruteForceAttempt = 61,
    UnusualLoginTime = 62,
    UnusualLocation = 63,
    TokenReuseAttempt = 64,

    // API events
    ApiKeyCreated = 70,
    ApiKeyRevoked = 71,
    ApiRateLimitExceeded = 72,

    // Admin actions
    AdminImpersonation = 80,
    SensitiveDataAccess = 81,
    ConfigurationChanged = 82
}

/// <summary>
/// Risk level for security events
/// </summary>
public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
