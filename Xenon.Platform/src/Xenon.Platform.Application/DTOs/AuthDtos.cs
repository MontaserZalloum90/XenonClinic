using System.ComponentModel.DataAnnotations;

namespace Xenon.Platform.Application.DTOs;

#region Platform Admin Auth

public record AdminLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record AdminLoginResponse
{
    public AdminUserDto User { get; init; } = null!;
    public string Token { get; init; } = string.Empty;
}

public record AdminUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; init; } = [];
}

#endregion

#region Tenant Auth

public record TenantLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record TenantLoginResponse
{
    public TenantSummaryDto Tenant { get; init; } = null!;
    public TenantUserDto User { get; init; } = null!;
    public string Token { get; init; } = string.Empty;
}

public record TenantSignupRequest
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; init; } = string.Empty;

    [Required]
    public string CompanyType { get; init; } = "Clinic";

    public string? ClinicType { get; init; }

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Phone]
    public string? Phone { get; init; }

    public string? Country { get; init; }
}

public record TenantSignupResponse
{
    public TenantSummaryDto Tenant { get; init; } = null!;
    public TenantUserDto User { get; init; } = null!;
    public string Token { get; init; } = string.Empty;
}

public record TenantSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? TrialEndDate { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
}

public record TenantUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public record TenantContextDto
{
    public TenantDetailDto Tenant { get; init; } = null!;
    public LicenseSummaryDto License { get; init; } = null!;
    public SubscriptionSummaryDto? Subscription { get; init; }
}

public record TenantDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string CompanyType { get; init; } = string.Empty;
    public string? ClinicType { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? TrialStartDate { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public bool IsTrialExpired { get; init; }
}

public record LicenseSummaryDto
{
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }
    public bool CanAddBranch { get; init; }
    public bool CanAddUser { get; init; }
}

public record SubscriptionSummaryDto
{
    public string PlanCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? DaysRemaining { get; init; }
    public string BillingCycle { get; init; } = string.Empty;
    public bool AutoRenew { get; init; }
}

#endregion

#region Refresh Tokens

public record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public record RefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

public record AdminLoginResponseWithRefresh : AdminLoginResponse
{
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
}

public record TenantLoginResponseWithRefresh : TenantLoginResponse
{
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }
}

#endregion

#region Password Management

public record ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record ChangePasswordResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public record ForgotPasswordResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record ResetPasswordResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}

public record ValidatePasswordRequest
{
    [Required]
    public string Password { get; init; } = string.Empty;
}

public record ValidatePasswordResponse
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public string Strength { get; init; } = string.Empty;
    public int StrengthScore { get; init; }
}

#endregion

#region Security Events

public record SecurityEventDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public bool IsSuccessful { get; init; }
    public string RiskLevel { get; init; } = string.Empty;
    public string? Details { get; init; }
}

public record LoginHistoryDto
{
    public IReadOnlyList<SecurityEventDto> Events { get; init; } = [];
    public int TotalSuccessfulLogins { get; init; }
    public int TotalFailedLogins { get; init; }
    public DateTime? LastSuccessfulLogin { get; init; }
    public DateTime? LastFailedLogin { get; init; }
}

public record ActiveSessionDto
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? DeviceFingerprint { get; init; }
    public bool IsCurrent { get; init; }
}

public record RevokeSessionRequest
{
    [Required]
    public Guid SessionId { get; init; }
}

public record RevokeAllSessionsRequest
{
    public bool KeepCurrent { get; init; } = true;
}

#endregion

#region Two-Factor Authentication

public record Enable2faRequest
{
    [Required]
    public string Password { get; init; } = string.Empty;
}

public record Enable2faResponse
{
    public string SharedKey { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
    public IReadOnlyList<string> BackupCodes { get; init; } = [];
}

public record Verify2faRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; init; } = string.Empty;
}

public record Verify2faResponse
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string>? BackupCodes { get; init; }
}

public record Disable2faRequest
{
    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; init; } = string.Empty;
}

public record TwoFactorLoginRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string TwoFactorToken { get; init; } = string.Empty;

    public bool UseBackupCode { get; init; }
}

#endregion
