namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Security configuration management service
/// </summary>
public interface ISecurityConfigurationService
{
    #region Security Settings

    /// <summary>
    /// Get security settings for a branch
    /// </summary>
    Task<SecuritySettingsDto> GetSecuritySettingsAsync(int branchId);

    /// <summary>
    /// Update security settings
    /// </summary>
    Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(int branchId, SecuritySettingsDto settings);

    #endregion

    #region Secrets Management

    /// <summary>
    /// Store a secret securely
    /// </summary>
    Task StoreSecretAsync(string key, string value, int branchId, DateTime? expiresAt = null);

    /// <summary>
    /// Retrieve a secret
    /// </summary>
    Task<string?> GetSecretAsync(string key, int branchId);

    /// <summary>
    /// Delete a secret
    /// </summary>
    Task DeleteSecretAsync(string key, int branchId);

    /// <summary>
    /// Rotate a secret
    /// </summary>
    Task RotateSecretAsync(string key, string newValue, int branchId);

    /// <summary>
    /// Get all secret keys (not values) for a branch
    /// </summary>
    Task<List<SecretMetadataDto>> GetSecretKeysAsync(int branchId);

    #endregion

    #region API Keys

    /// <summary>
    /// Create an API key
    /// </summary>
    Task<ApiKeyDto> CreateApiKeyAsync(CreateApiKeyDto request);

    /// <summary>
    /// Get API key by key value
    /// </summary>
    Task<ApiKeyDto?> GetApiKeyAsync(string apiKey);

    /// <summary>
    /// Get all API keys for a branch
    /// </summary>
    Task<List<ApiKeyDto>> GetApiKeysAsync(int branchId);

    /// <summary>
    /// Revoke an API key
    /// </summary>
    Task RevokeApiKeyAsync(int apiKeyId);

    /// <summary>
    /// Validate an API key and return associated permissions
    /// </summary>
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey);

    #endregion

    #region Password Policy

    /// <summary>
    /// Get password policy for a branch
    /// </summary>
    Task<PasswordPolicyDto> GetPasswordPolicyAsync(int branchId);

    /// <summary>
    /// Update password policy
    /// </summary>
    Task<PasswordPolicyDto> UpdatePasswordPolicyAsync(int branchId, PasswordPolicyDto policy);

    /// <summary>
    /// Validate password against policy
    /// </summary>
    Task<PasswordValidationResult> ValidatePasswordAsync(string password, int branchId, int? userId = null);

    /// <summary>
    /// Record password in history (for reuse prevention)
    /// </summary>
    Task RecordPasswordHistoryAsync(int userId, string passwordHash);

    #endregion
}

/// <summary>
/// Security settings DTO
/// </summary>
public class SecuritySettingsDto
{
    public int BranchId { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool RequireMfa { get; set; }
    public List<string> AllowedMfaMethods { get; set; } = new() { "TOTP", "SMS", "Email" };
    public bool RequirePasswordChange { get; set; }
    public int PasswordChangeDays { get; set; } = 90;
    public bool EnforceIpWhitelist { get; set; }
    public List<string> IpWhitelist { get; set; } = new();
    public bool EnableAuditLogging { get; set; } = true;
    public bool EnablePHIEncryption { get; set; } = true;
    public int AuditRetentionDays { get; set; } = 2555; // 7 years for HIPAA
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Secret metadata (without the actual value)
/// </summary>
public class SecretMetadataDto
{
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public bool IsExpired { get; set; }
}

/// <summary>
/// API key DTO
/// </summary>
public class ApiKeyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? KeyPrefix { get; set; } // Only show first 8 chars
    public int BranchId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
    public int? RateLimitPerMinute { get; set; }
}

/// <summary>
/// Create API key request
/// </summary>
public class CreateApiKeyDto
{
    public string Name { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
    public int? RateLimitPerMinute { get; set; }
}

/// <summary>
/// API key validation result
/// </summary>
public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public int? BranchId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public string? InvalidReason { get; set; }
    public bool IsRateLimited { get; set; }
}

/// <summary>
/// Password policy DTO
/// </summary>
public class PasswordPolicyDto
{
    public int BranchId { get; set; }
    public int MinLength { get; set; } = 12;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int PasswordHistoryCount { get; set; } = 12; // Can't reuse last 12 passwords
    public int MaxAgeDays { get; set; } = 90;
    public int MinAgeDays { get; set; } = 1; // Prevent rapid changes
    public List<string> DisallowedPatterns { get; set; } = new() { "password", "123456", "qwerty" };
}

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StrengthScore { get; set; } // 0-100
    public string StrengthLabel { get; set; } = string.Empty; // Weak, Fair, Good, Strong
}
