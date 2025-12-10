namespace Xenon.Platform.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT token renewal.
/// Supports both platform admins and tenant admins.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The actual token value (hashed for security)
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Type of token: "PlatformAdmin" or "TenantAdmin"
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// User ID (either PlatformAdmin.Id or TenantAdmin.Id)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional tenant ID for tenant admin tokens
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// When the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the token was last used to refresh
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// IP address that created the token
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address that last used the token
    /// </summary>
    public string? LastUsedByIp { get; set; }

    /// <summary>
    /// User agent that created the token
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Device fingerprint for enhanced security
    /// </summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// Whether the token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When the token was revoked
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for revocation
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Token that replaced this one (for token rotation)
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Check if the token is active (not expired, not revoked)
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired && !IsDeleted;

    /// <summary>
    /// Check if the token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Revoke the token with a reason
    /// </summary>
    public void Revoke(string reason, Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        ReplacedByTokenId = replacedByTokenId;
    }
}

/// <summary>
/// Types of refresh tokens
/// </summary>
public static class RefreshTokenTypes
{
    public const string PlatformAdmin = "PlatformAdmin";
    public const string TenantAdmin = "TenantAdmin";
}
