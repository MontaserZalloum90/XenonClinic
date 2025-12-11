namespace Xenon.Platform.Domain.Entities;

public class TenantAdmin : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = "TENANT_ADMIN"; // TENANT_ADMIN, TENANT_MANAGER, TENANT_USER

    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime? LastLogoutAt { get; set; }
    public DateTime? TokenInvalidatedAt { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt > DateTime.UtcNow;
}
