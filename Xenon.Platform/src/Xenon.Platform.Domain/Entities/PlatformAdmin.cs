namespace Xenon.Platform.Domain.Entities;

public class PlatformAdmin : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = "PLATFORM_ADMIN"; // SUPER_ADMIN, PLATFORM_ADMIN, SUPPORT

    // Permissions stored as comma-separated values
    public string Permissions { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public DateTime? LastLogoutAt { get; set; }
    public DateTime? TokenInvalidatedAt { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsLockedOut => LockoutEndAt.HasValue && LockoutEndAt > DateTime.UtcNow;

    public IEnumerable<string> GetPermissions() =>
        string.IsNullOrEmpty(Permissions)
            ? Enumerable.Empty<string>()
            : Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);

    public bool HasPermission(string permission) =>
        Role == "SUPER_ADMIN" || GetPermissions().Contains(permission);
}
