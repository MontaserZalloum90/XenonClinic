namespace Xenon.Platform.Domain.Entities;

/// <summary>
/// Represents a password reset token for account recovery.
/// </summary>
public class PasswordResetToken : BaseEntity
{
    /// <summary>
    /// Hashed token value for security
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Type of user: "PlatformAdmin" or "TenantAdmin"
    /// </summary>
    public string UserType { get; set; } = string.Empty;

    /// <summary>
    /// The user ID requesting the reset
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email address for the reset request
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the token expires (typically 24 hours)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the token has been used
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// When the token was used
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// IP address that requested the reset
    /// </summary>
    public string? RequestedByIp { get; set; }

    /// <summary>
    /// IP address that used the token
    /// </summary>
    public string? UsedByIp { get; set; }

    /// <summary>
    /// User agent that requested the reset
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Check if the token is valid (not expired, not used)
    /// </summary>
    public bool IsValid => !IsUsed && !IsExpired && !IsDeleted;

    /// <summary>
    /// Check if the token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Mark the token as used
    /// </summary>
    public void MarkAsUsed(string? usedByIp = null)
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UsedByIp = usedByIp;
    }
}
