using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Entity configurations for security and compliance entities
/// </summary>
public static class SecurityEntityConfiguration
{
    /// <summary>
    /// Apply security entity configurations to the model builder
    /// </summary>
    public static void ConfigureSecurityEntities(this ModelBuilder builder)
    {
        // ========================================
        // Audit Log Configuration
        // ========================================
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EventCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ResourceId).HasMaxLength(100);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestPath).HasMaxLength(500);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.IntegrityHash).HasMaxLength(128);

            // Indexes for common queries
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.EventCategory);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.BranchId);
            entity.HasIndex(e => e.IsPHIAccess);
            entity.HasIndex(e => e.IsEmergencyAccess);
            entity.HasIndex(e => new { e.BranchId, e.Timestamp });
            entity.HasIndex(e => new { e.PatientId, e.Timestamp });
        });

        builder.Entity<AuditRetentionPolicy>(entity =>
        {
            entity.ToTable("AuditRetentionPolicies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ArchiveLocation).HasMaxLength(500);
            entity.HasIndex(e => e.EventCategory).IsUnique();
        });

        builder.Entity<AuditAlertConfig>(entity =>
        {
            entity.ToTable("AuditAlertConfigs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Condition).HasMaxLength(500);
            entity.Property(e => e.Severity).HasMaxLength(20);
        });

        // ========================================
        // RBAC Configuration
        // ========================================
        builder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RoleType).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.RoleType);
            entity.HasIndex(e => e.BranchId);
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
        });

        builder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => new { e.UserId, e.PermissionId });
            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
        });

        builder.Entity<DataAccessRule>(entity =>
        {
            entity.ToTable("DataAccessRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Condition).HasMaxLength(1000);
            entity.HasIndex(e => e.ResourceType);
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.Priority);
        });

        // ========================================
        // Patient Consent Configuration
        // ========================================
        builder.Entity<PatientConsent>(entity =>
        {
            entity.ToTable("PatientConsents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConsentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConsentCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.Property(e => e.WitnessName).HasMaxLength(200);
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.ConsentType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.PatientId, e.ConsentType });
            entity.HasIndex(e => e.ExpirationDate);
        });

        builder.Entity<ConsentHistory>(entity =>
        {
            entity.ToTable("ConsentHistory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PreviousStatus).HasMaxLength(20);
            entity.Property(e => e.NewStatus).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasOne(e => e.Consent)
                .WithMany(c => c.History)
                .HasForeignKey(e => e.ConsentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ConsentId);
            entity.HasIndex(e => e.ActionDate);
        });

        builder.Entity<ConsentFormTemplate>(entity =>
        {
            entity.ToTable("ConsentFormTemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ConsentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConsentCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("en");
            entity.HasIndex(e => e.ConsentType);
            entity.HasIndex(e => new { e.ConsentType, e.Language });
        });

        // ========================================
        // Security Configuration Entities
        // ========================================
        builder.Entity<SecuritySettingsEntity>(entity =>
        {
            entity.ToTable("SecuritySettings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BranchId).IsUnique();
        });

        builder.Entity<SecretEntity>(entity =>
        {
            entity.ToTable("Secrets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EncryptedValue).IsRequired();
            entity.HasIndex(e => new { e.BranchId, e.Key }).IsUnique();
        });

        builder.Entity<ApiKeyEntity>(entity =>
        {
            entity.ToTable("ApiKeys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.KeyHash).HasMaxLength(128).IsRequired();
            entity.Property(e => e.KeyPrefix).HasMaxLength(12);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.BranchId);
        });

        builder.Entity<PasswordHistoryEntity>(entity =>
        {
            entity.ToTable("PasswordHistory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // ========================================
        // Backup Configuration
        // ========================================
        builder.Entity<BackupRecord>(entity =>
        {
            entity.ToTable("BackupRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BackupType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Checksum).HasMaxLength(128);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.BranchId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
        });
    }
}

// ========================================
// Security Entity Classes
// ========================================

public class AuditLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public string? UserEmail { get; set; }
    public int? PatientId { get; set; }
    public bool IsPHIAccess { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? HttpMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedFields { get; set; }
    public string? Reason { get; set; }
    public bool IsEmergencyAccess { get; set; }
    public string? EmergencyJustification { get; set; }
    public int BranchId { get; set; }
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? AdditionalData { get; set; }
    public string? IntegrityHash { get; set; }
    public string? ModuleName { get; set; }
}

public class AuditRetentionPolicy
{
    public int Id { get; set; }
    public string EventCategory { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 2555; // 7 years for HIPAA
    public bool ArchiveBeforeDelete { get; set; } = true;
    public string? ArchiveLocation { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AuditAlertConfig
{
    public int Id { get; set; }
    public string AlertName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public int Threshold { get; set; }
    public int TimeWindowMinutes { get; set; }
    public string? Severity { get; set; }
    public string? NotifyEmails { get; set; } // JSON array
    public bool IsActive { get; set; } = true;
}

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public bool IsPHIRelated { get; set; }
    public bool IsSystemPermission { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
    public int? BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedBy { get; set; }
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedBy { get; set; }
}

public class UserPermission
{
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedBy { get; set; }
}

public class DataAccessRule
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public int? RoleId { get; set; }
    public bool AllowAccess { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PatientConsent
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? GrantedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? GrantedBy { get; set; }
    public int? RevokedBy { get; set; }
    public string? RevokedReason { get; set; }
    public string? DocumentPath { get; set; }
    public string? SignatureData { get; set; }
    public string? WitnessName { get; set; }
    public string? ScopeJson { get; set; }
    public int BranchId { get; set; }
    public ICollection<ConsentHistory> History { get; set; } = new List<ConsentHistory>();
}

public class ConsentHistory
{
    public int Id { get; set; }
    public int ConsentId { get; set; }
    public PatientConsent Consent { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public int? ActionByUserId { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
}

public class ConsentFormTemplate
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateContent { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool RequiresWitness { get; set; }
    public int? ValidityDays { get; set; }
    public string? RequiredFieldsJson { get; set; }
    public int? BranchId { get; set; }
}

public class SecuritySettingsEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public bool RequireMfa { get; set; }
    public string? AllowedMfaMethods { get; set; } // JSON
    public bool RequirePasswordChange { get; set; }
    public int PasswordChangeDays { get; set; } = 90;
    public bool EnforceIpWhitelist { get; set; }
    public string? IpWhitelist { get; set; } // JSON
    public bool EnableAuditLogging { get; set; } = true;
    public bool EnablePHIEncryption { get; set; } = true;
    public int AuditRetentionDays { get; set; } = 2555;
    public DateTime? LastUpdated { get; set; }
}

public class SecretEntity
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string EncryptedValue { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public class ApiKeyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string? KeyPrefix { get; set; }
    public int BranchId { get; set; }
    public string? Permissions { get; set; } // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RateLimitPerMinute { get; set; }
}

public class PasswordHistoryEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BackupRecord
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Checksum { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
