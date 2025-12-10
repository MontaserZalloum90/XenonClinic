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
