namespace XenonClinic.Infrastructure.Entities;

/// <summary>
/// Stores MFA configuration for users - persisted to database.
/// </summary>
public class UserMfaConfiguration
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? TotpSecret { get; set; }
    public bool IsEnabled { get; set; }
    public int EnabledMethod { get; set; } // Maps to MfaMethod enum
    public DateTime? EnabledAt { get; set; }
    public string? BackupCodesJson { get; set; } // JSON serialized list of hashed backup codes
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ApplicationUser? User { get; set; }
}
