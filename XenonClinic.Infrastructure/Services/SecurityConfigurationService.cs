using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Security configuration and settings
/// </summary>
public class SecuritySettings
{
    // Authentication
    public int PasswordMinLength { get; set; } = 12;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int PasswordExpirationDays { get; set; } = 90;
    public int PasswordHistoryCount { get; set; } = 12;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;

    // Session
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int AbsoluteSessionTimeoutHours { get; set; } = 12;
    public bool RequireMfaForAdmin { get; set; } = true;
    public bool RequireMfaForPHI { get; set; } = false;

    // API Security
    public int ApiRateLimitPerMinute { get; set; } = 100;
    public int ApiRateLimitPerHour { get; set; } = 1000;
    public bool RequireApiKey { get; set; } = true;
    public int ApiKeyExpirationDays { get; set; } = 365;

    // Data Protection
    public bool EncryptPHIAtRest { get; set; } = true;
    public bool EncryptBackups { get; set; } = true;
    public bool MaskPHIInLogs { get; set; } = true;
    public int DataRetentionYears { get; set; } = 7;

    // Audit
    public bool EnableAuditLogging { get; set; } = true;
    public bool LogPHIAccess { get; set; } = true;
    public int AuditRetentionYears { get; set; } = 7;
}

/// <summary>
/// Secret types
/// </summary>
public enum SecretType
{
    EncryptionKey,
    ApiKey,
    OAuthClientSecret,
    DatabasePassword,
    SmtpPassword,
    WebhookSecret,
    IntegrationCredential
}

/// <summary>
/// Internal security configuration service interface (Infrastructure layer)
/// Note: This is separate from Core.Interfaces.ISecurityConfigurationService which has a different contract
/// </summary>
public interface IInternalSecurityConfigurationService
{
    // Settings
    Task<SecuritySettings> GetSecuritySettingsAsync();
    Task<SecuritySettings> UpdateSecuritySettingsAsync(SecuritySettings settings, int userId);

    // Secrets Management
    Task<string> GetSecretAsync(string secretName);
    Task SetSecretAsync(string secretName, string secretValue, SecretType type, int userId, DateTime? expiresAt = null);
    Task<bool> DeleteSecretAsync(string secretName, int userId);
    Task<List<SecretInfo>> GetSecretsInfoAsync();
    Task RotateSecretAsync(string secretName, int userId);
    Task<List<SecretInfo>> GetExpiringSecretsAsync(int daysAhead = 30);

    // API Keys
    Task<ApiKeyInfo> GenerateApiKeyAsync(string name, string[] scopes, int userId, int? expirationDays = null);
    Task<ApiKeyValidation> ValidateApiKeyAsync(string apiKey);
    Task<bool> RevokeApiKeyAsync(int apiKeyId, int userId);
    Task<List<ApiKeyInfo>> GetApiKeysAsync();

    // Password Policy
    Task<PasswordValidationResult> ValidatePasswordAsync(string password, int? userId = null);
    Task<bool> IsPasswordInHistoryAsync(int userId, string passwordHash);
    Task AddPasswordToHistoryAsync(int userId, string passwordHash);

    // Security Health
    Task<SecurityHealthReport> GetSecurityHealthReportAsync();
}

/// <summary>
/// Secret information (without actual secret value)
/// </summary>
public class SecretInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SecretType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastRotatedAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public int DaysUntilExpiration => ExpiresAt.HasValue ? (int)(ExpiresAt.Value - DateTime.UtcNow).TotalDays : int.MaxValue;
}

/// <summary>
/// API key information
/// </summary>
public class ApiKeyInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? KeyPrefix { get; set; }
    public string? FullKey { get; set; } // Only populated on creation
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// API key validation result
/// </summary>
public class ApiKeyValidation
{
    public bool IsValid { get; set; }
    public int? ApiKeyId { get; set; }
    public string? Name { get; set; }
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StrengthScore { get; set; } // 0-100
}

/// <summary>
/// Security health report
/// </summary>
public class SecurityHealthReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public int OverallScore { get; set; } // 0-100
    public string Status { get; set; } = string.Empty; // Good, Warning, Critical
    public List<SecurityCheckResult> Checks { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Individual security check result
/// </summary>
public class SecurityCheckResult
{
    public string CheckName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Details { get; set; }
    public string Severity { get; set; } = "Info";
}

/// <summary>
/// Security configuration service implementation
/// </summary>
public class SecurityConfigurationService : IInternalSecurityConfigurationService
{
    private readonly ClinicDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityConfigurationService> _logger;
    private readonly byte[] _secretEncryptionKey;

    public SecurityConfigurationService(ClinicDbContext context, IConfiguration configuration, ILogger<SecurityConfigurationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        
        var keyBase64 = configuration["Security:SecretEncryptionKey"] ?? throw new InvalidOperationException("Secret encryption key not configured");
        _secretEncryptionKey = Convert.FromBase64String(keyBase64);
    }

    #region Security Settings

    public async Task<SecuritySettings> GetSecuritySettingsAsync()
    {
        var setting = await _context.Set<SecuritySettingsEntity>().FirstOrDefaultAsync();
        if (setting == null)
        {
            return new SecuritySettings();
        }
        return JsonSerializer.Deserialize<SecuritySettings>(setting.SettingsJson) ?? new SecuritySettings();
    }

    public async Task<SecuritySettings> UpdateSecuritySettingsAsync(SecuritySettings settings, int userId)
    {
        var entity = await _context.Set<SecuritySettingsEntity>().FirstOrDefaultAsync();
        if (entity == null)
        {
            entity = new SecuritySettingsEntity { CreatedAt = DateTime.UtcNow };
            _context.Set<SecuritySettingsEntity>().Add(entity);
        }

        entity.SettingsJson = JsonSerializer.Serialize(settings);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Security settings updated by user {UserId}", userId);

        return settings;
    }

    #endregion

    #region Secrets Management

    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _context.Set<SecretEntity>().FirstOrDefaultAsync(s => s.Name == secretName && s.IsActive);
        if (secret == null)
            throw new InvalidOperationException($"Secret '{secretName}' not found");

        if (secret.ExpiresAt.HasValue && secret.ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException($"Secret '{secretName}' has expired");

        return DecryptSecret(secret.EncryptedValue);
    }

    public async Task SetSecretAsync(string secretName, string secretValue, SecretType type, int userId, DateTime? expiresAt = null)
    {
        var existing = await _context.Set<SecretEntity>().FirstOrDefaultAsync(s => s.Name == secretName);
        
        if (existing != null)
        {
            existing.EncryptedValue = EncryptSecret(secretValue);
            existing.Type = type.ToString();
            existing.ExpiresAt = expiresAt;
            existing.LastRotatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;
        }
        else
        {
            var secret = new SecretEntity
            {
                Name = secretName,
                EncryptedValue = EncryptSecret(secretValue),
                Type = type.ToString(),
                ExpiresAt = expiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            _context.Set<SecretEntity>().Add(secret);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Secret '{SecretName}' saved by user {UserId}", secretName, userId);
    }

    public async Task<bool> DeleteSecretAsync(string secretName, int userId)
    {
        var secret = await _context.Set<SecretEntity>().FirstOrDefaultAsync(s => s.Name == secretName);
        if (secret == null) return false;

        secret.IsActive = false;
        secret.UpdatedByUserId = userId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Secret '{SecretName}' deleted by user {UserId}", secretName, userId);
        return true;
    }

    public async Task<List<SecretInfo>> GetSecretsInfoAsync()
    {
        var secrets = await _context.Set<SecretEntity>().Where(s => s.IsActive).ToListAsync();
        return secrets.Select(s => new SecretInfo
        {
            Id = s.Id,
            Name = s.Name,
            Type = Enum.Parse<SecretType>(s.Type),
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt,
            LastRotatedAt = s.LastRotatedAt
        }).ToList();
    }

    public async Task RotateSecretAsync(string secretName, int userId)
    {
        var secret = await _context.Set<SecretEntity>().FirstOrDefaultAsync(s => s.Name == secretName && s.IsActive);
        if (secret == null)
            throw new InvalidOperationException($"Secret '{secretName}' not found");

        // Generate new secret value
        var newValue = GenerateSecureToken(32);
        secret.EncryptedValue = EncryptSecret(newValue);
        secret.LastRotatedAt = DateTime.UtcNow;
        secret.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Secret '{SecretName}' rotated by user {UserId}", secretName, userId);
    }

    public async Task<List<SecretInfo>> GetExpiringSecretsAsync(int daysAhead = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(daysAhead);
        var secrets = await _context.Set<SecretEntity>()
            .Where(s => s.IsActive && s.ExpiresAt != null && s.ExpiresAt <= cutoff)
            .ToListAsync();

        return secrets.Select(s => new SecretInfo
        {
            Id = s.Id,
            Name = s.Name,
            Type = Enum.Parse<SecretType>(s.Type),
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt,
            LastRotatedAt = s.LastRotatedAt
        }).ToList();
    }

    #endregion

    #region API Keys

    public async Task<ApiKeyInfo> GenerateApiKeyAsync(string name, string[] scopes, int userId, int? expirationDays = null)
    {
        var keyValue = GenerateSecureToken(32);
        var keyHash = HashApiKey(keyValue);
        var keyPrefix = keyValue[..8];

        var apiKey = new ApiKeyEntity
        {
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            ScopesJson = JsonSerializer.Serialize(scopes),
            ExpiresAt = expirationDays.HasValue ? DateTime.UtcNow.AddDays(expirationDays.Value) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.Set<ApiKeyEntity>().Add(apiKey);
        await _context.SaveChangesAsync();

        _logger.LogInformation("API key '{Name}' generated by user {UserId}", name, userId);

        return new ApiKeyInfo
        {
            Id = apiKey.Id,
            Name = name,
            KeyPrefix = keyPrefix,
            FullKey = keyValue, // Only returned on creation!
            Scopes = scopes,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt,
            IsActive = true
        };
    }

    public async Task<ApiKeyValidation> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
            return new ApiKeyValidation { IsValid = false, ErrorMessage = "Invalid API key format" };

        var keyHash = HashApiKey(apiKey);
        var entity = await _context.Set<ApiKeyEntity>().FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive);

        if (entity == null)
            return new ApiKeyValidation { IsValid = false, ErrorMessage = "API key not found" };

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt.Value < DateTime.UtcNow)
            return new ApiKeyValidation { IsValid = false, ErrorMessage = "API key has expired" };

        // Update usage statistics
        entity.LastUsedAt = DateTime.UtcNow;
        entity.UsageCount++;
        await _context.SaveChangesAsync();

        return new ApiKeyValidation
        {
            IsValid = true,
            ApiKeyId = entity.Id,
            Name = entity.Name,
            Scopes = JsonSerializer.Deserialize<string[]>(entity.ScopesJson ?? "[]") ?? Array.Empty<string>()
        };
    }

    public async Task<bool> RevokeApiKeyAsync(int apiKeyId, int userId)
    {
        var apiKey = await _context.Set<ApiKeyEntity>().FindAsync(apiKeyId);
        if (apiKey == null) return false;

        apiKey.IsActive = false;
        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.RevokedByUserId = userId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("API key {ApiKeyId} revoked by user {UserId}", apiKeyId, userId);

        return true;
    }

    public async Task<List<ApiKeyInfo>> GetApiKeysAsync()
    {
        var keys = await _context.Set<ApiKeyEntity>().OrderByDescending(k => k.CreatedAt).ToListAsync();
        return keys.Select(k => new ApiKeyInfo
        {
            Id = k.Id,
            Name = k.Name,
            KeyPrefix = k.KeyPrefix,
            Scopes = JsonSerializer.Deserialize<string[]>(k.ScopesJson ?? "[]") ?? Array.Empty<string>(),
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt,
            LastUsedAt = k.LastUsedAt,
            IsActive = k.IsActive,
            UsageCount = k.UsageCount
        }).ToList();
    }

    #endregion

    #region Password Policy

    public async Task<PasswordValidationResult> ValidatePasswordAsync(string password, int? userId = null)
    {
        var settings = await GetSecuritySettingsAsync();
        var result = new PasswordValidationResult { IsValid = true };

        if (password.Length < settings.PasswordMinLength)
        {
            result.IsValid = false;
            result.Errors.Add($"Password must be at least {settings.PasswordMinLength} characters");
        }

        if (settings.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one uppercase letter");
        }

        if (settings.RequireLowercase && !password.Any(char.IsLower))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one lowercase letter");
        }

        if (settings.RequireDigit && !password.Any(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one digit");
        }

        if (settings.RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one special character");
        }

        // Check password history if user provided
        if (userId.HasValue)
        {
            var passwordHash = HashApiKey(password); // Reuse hash function
            if (await IsPasswordInHistoryAsync(userId.Value, passwordHash))
            {
                result.IsValid = false;
                result.Errors.Add($"Password was used in the last {settings.PasswordHistoryCount} passwords");
            }
        }

        // Calculate strength score
        result.StrengthScore = CalculatePasswordStrength(password);

        return result;
    }

    public async Task<bool> IsPasswordInHistoryAsync(int userId, string passwordHash)
    {
        var settings = await GetSecuritySettingsAsync();
        var recentPasswords = await _context.Set<PasswordHistoryEntity>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(settings.PasswordHistoryCount)
            .Select(p => p.PasswordHash)
            .ToListAsync();

        return recentPasswords.Contains(passwordHash);
    }

    public async Task AddPasswordToHistoryAsync(int userId, string passwordHash)
    {
        var history = new PasswordHistoryEntity
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<PasswordHistoryEntity>().Add(history);
        await _context.SaveChangesAsync();
    }

    private static int CalculatePasswordStrength(string password)
    {
        var score = 0;
        if (password.Length >= 8) score += 20;
        if (password.Length >= 12) score += 20;
        if (password.Any(char.IsUpper)) score += 15;
        if (password.Any(char.IsLower)) score += 15;
        if (password.Any(char.IsDigit)) score += 15;
        if (password.Any(c => !char.IsLetterOrDigit(c))) score += 15;
        return Math.Min(score, 100);
    }

    #endregion

    #region Security Health

    public async Task<SecurityHealthReport> GetSecurityHealthReportAsync()
    {
        var report = new SecurityHealthReport();
        var checks = new List<SecurityCheckResult>();
        var settings = await GetSecuritySettingsAsync();

        // Check encryption settings
        checks.Add(new SecurityCheckResult
        {
            CheckName = "PHI Encryption at Rest",
            Category = "Data Protection",
            Passed = settings.EncryptPHIAtRest,
            Details = settings.EncryptPHIAtRest ? "Enabled" : "DISABLED - PHI may be exposed",
            Severity = settings.EncryptPHIAtRest ? "Info" : "Critical"
        });

        // Check password policy
        checks.Add(new SecurityCheckResult
        {
            CheckName = "Password Policy Strength",
            Category = "Authentication",
            Passed = settings.PasswordMinLength >= 12,
            Details = $"Minimum length: {settings.PasswordMinLength}",
            Severity = settings.PasswordMinLength >= 12 ? "Info" : "Warning"
        });

        // Check MFA settings
        checks.Add(new SecurityCheckResult
        {
            CheckName = "MFA for Admin Users",
            Category = "Authentication",
            Passed = settings.RequireMfaForAdmin,
            Details = settings.RequireMfaForAdmin ? "Required" : "Not required",
            Severity = settings.RequireMfaForAdmin ? "Info" : "Warning"
        });

        // Check audit logging
        checks.Add(new SecurityCheckResult
        {
            CheckName = "Audit Logging",
            Category = "Compliance",
            Passed = settings.EnableAuditLogging && settings.LogPHIAccess,
            Details = settings.EnableAuditLogging ? "Enabled" : "DISABLED",
            Severity = settings.EnableAuditLogging ? "Info" : "Critical"
        });

        // Check expiring secrets
        var expiringSecrets = await GetExpiringSecretsAsync(30);
        checks.Add(new SecurityCheckResult
        {
            CheckName = "Expiring Secrets",
            Category = "Secrets Management",
            Passed = !expiringSecrets.Any(),
            Details = expiringSecrets.Any() ? $"{expiringSecrets.Count} secrets expiring soon" : "No secrets expiring soon",
            Severity = expiringSecrets.Any() ? "Warning" : "Info"
        });

        report.Checks = checks;
        
        // Calculate overall score
        var criticalFailed = checks.Count(c => !c.Passed && c.Severity == "Critical");
        var warningFailed = checks.Count(c => !c.Passed && c.Severity == "Warning");
        
        report.OverallScore = Math.Max(0, 100 - (criticalFailed * 30) - (warningFailed * 10));
        report.Status = report.OverallScore >= 80 ? "Good" : report.OverallScore >= 50 ? "Warning" : "Critical";

        if (!settings.RequireMfaForAdmin)
            report.Recommendations.Add("Enable MFA for administrator accounts");
        if (expiringSecrets.Any())
            report.Recommendations.Add("Rotate expiring secrets before they expire");
        if (!settings.EncryptBackups)
            report.Recommendations.Add("Enable backup encryption for data protection");

        return report;
    }

    #endregion

    #region Helper Methods

    private string EncryptSecret(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _secretEncryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encrypted.Length];
        aes.IV.CopyTo(result, 0);
        encrypted.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    private string DecryptSecret(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _secretEncryptionKey;

        var iv = fullCipher.Take(16).ToArray();
        var cipher = fullCipher.Skip(16).ToArray();
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(decrypted);
    }

    private static string GenerateSecureToken(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hash);
    }

    #endregion
}

#region Security Entities

[Table("SecuritySettings")]
public class SecuritySettingsEntity
{
    [Key] public int Id { get; set; }
    [Required] public string SettingsJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

[Table("Secrets")]
public class SecretEntity
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required] public string EncryptedValue { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Type { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastRotatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
}

[Table("ApiKeys")]
public class ApiKeyEntity
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string KeyHash { get; set; } = string.Empty;
    [MaxLength(20)] public string? KeyPrefix { get; set; }
    public string? ScopesJson { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? RevokedByUserId { get; set; }
}

[Table("PasswordHistory")]
public class PasswordHistoryEntity
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(100)] public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

#endregion
