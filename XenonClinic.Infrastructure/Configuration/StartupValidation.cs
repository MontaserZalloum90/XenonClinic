using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// Startup validation to ensure all required configuration is present
/// </summary>
public static class StartupValidation
{
    /// <summary>
    /// Validates required configuration settings at startup
    /// </summary>
    public static IServiceCollection ValidateConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var errors = new List<string>();

        // Database connection
        var connectionString = configuration.GetConnectionString("ClinicDb")
            ?? configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Database connection string is required. Configure 'ConnectionStrings:ClinicDb' or 'ConnectionStrings:DefaultConnection'.");
        }

        // Security configuration
        var encryptionKey = configuration["Security:EncryptionMasterKey"];
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            errors.Add("Security:EncryptionMasterKey is required for PHI encryption.");
        }
        else if (encryptionKey.Length < 32)
        {
            errors.Add("Security:EncryptionMasterKey must be at least 32 characters for AES-256 encryption.");
        }

        // JWT configuration
        var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            errors.Add("JWT SecretKey is required. Set JWT_SECRET_KEY environment variable or Jwt:SecretKey in configuration.");
        }
        else if (jwtSecretKey.Length < 32)
        {
            errors.Add("JWT SecretKey must be at least 32 characters for security.");
        }

        // Throw if critical errors
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"  - {e}")));
        }

        return services;
    }

    /// <summary>
    /// Validates and logs configuration warnings (non-critical)
    /// </summary>
    public static IServiceCollection ValidateConfigurationWithWarnings(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger? logger = null)
    {
        var warnings = new List<string>();

        // Backup configuration
        var backupPath = configuration["Backup:Path"];
        if (string.IsNullOrWhiteSpace(backupPath))
        {
            warnings.Add("Backup:Path not configured. Using default path.");
        }

        // Redis configuration (optional)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            warnings.Add("Redis connection string not configured. Using in-memory caching.");
        }

        // Email configuration (optional but recommended)
        var smtpHost = configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            warnings.Add("Email:SmtpHost not configured. Email notifications will be disabled.");
        }

        // Log warnings
        if (warnings.Count > 0 && logger != null)
        {
            foreach (var warning in warnings)
            {
                logger.LogWarning("Configuration warning: {Warning}", warning);
            }
        }

        return services;
    }

    /// <summary>
    /// Gets security configuration with defaults
    /// </summary>
    public static SecurityConfiguration GetSecurityConfiguration(this IConfiguration configuration)
    {
        return new SecurityConfiguration
        {
            EncryptionMasterKey = configuration["Security:EncryptionMasterKey"] ?? string.Empty,
            JwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? configuration["Jwt:SecretKey"] ?? string.Empty,
            JwtIssuer = configuration["Jwt:Issuer"] ?? "xenon-clinic",
            JwtAudience = configuration["Jwt:Audience"] ?? "xenon-clinic-api",
            JwtExpirationMinutes = configuration.GetValue("Jwt:ExpirationMinutes", 60),
            SessionTimeoutMinutes = configuration.GetValue("Security:SessionTimeoutMinutes", 30),
            MaxLoginAttempts = configuration.GetValue("Security:MaxLoginAttempts", 5),
            LockoutDurationMinutes = configuration.GetValue("Security:LockoutDurationMinutes", 15),
            RequireMfa = configuration.GetValue("Security:RequireMfa", false),
            PasswordMinLength = configuration.GetValue("Security:PasswordMinLength", 12),
            PasswordHistoryCount = configuration.GetValue("Security:PasswordHistoryCount", 12),
            AuditRetentionDays = configuration.GetValue("Security:AuditRetentionDays", 2555) // 7 years for HIPAA
        };
    }

    /// <summary>
    /// Gets backup configuration with defaults
    /// </summary>
    public static BackupConfiguration GetBackupConfiguration(this IConfiguration configuration)
    {
        return new BackupConfiguration
        {
            BackupPath = configuration["Backup:Path"] ?? "/var/backups/xenon-clinic",
            FullBackupSchedule = configuration["Backup:FullBackupSchedule"] ?? "0 2 * * 0", // 2 AM Sunday
            IncrementalBackupSchedule = configuration["Backup:IncrementalBackupSchedule"] ?? "0 2 * * 1-6", // 2 AM Mon-Sat
            RetentionDays = configuration.GetValue("Backup:RetentionDays", 90),
            EncryptBackups = configuration.GetValue("Backup:EncryptBackups", true),
            CompressBackups = configuration.GetValue("Backup:CompressBackups", true)
        };
    }
}

/// <summary>
/// Security configuration settings
/// </summary>
public class SecurityConfiguration
{
    public string EncryptionMasterKey { get; set; } = string.Empty;
    public string JwtSecretKey { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = string.Empty;
    public int JwtExpirationMinutes { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public bool RequireMfa { get; set; }
    public int PasswordMinLength { get; set; }
    public int PasswordHistoryCount { get; set; }
    public int AuditRetentionDays { get; set; }
}

/// <summary>
/// Backup configuration settings
/// </summary>
public class BackupConfiguration
{
    public string BackupPath { get; set; } = string.Empty;
    public string FullBackupSchedule { get; set; } = string.Empty;
    public string IncrementalBackupSchedule { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public bool EncryptBackups { get; set; }
    public bool CompressBackups { get; set; }
}
