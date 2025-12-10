using System.ComponentModel.DataAnnotations;

namespace Xenon.Platform.Application.DTOs;

#region Admin Tenant Management

public record TenantListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string CompanyType { get; init; } = string.Empty;
    public string? ClinicType { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string? Country { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }
    public bool IsDatabaseProvisioned { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TenantListQuery
{
    [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters")]
    public string? Search { get; init; }

    [StringLength(50, ErrorMessage = "Status filter cannot exceed 50 characters")]
    public string? Status { get; init; }

    [StringLength(50, ErrorMessage = "CompanyType filter cannot exceed 50 characters")]
    public string? CompanyType { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; init; } = 20;
}

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public record TenantFullDetailsDto
{
    public TenantInfoDto Tenant { get; init; } = null!;
    public IReadOnlyList<TenantAdminDto> Admins { get; init; } = [];
    public IReadOnlyList<TenantSubscriptionDto> Subscriptions { get; init; } = [];
    public IReadOnlyList<TenantHealthHistoryDto> HealthHistory { get; init; } = [];
    public IReadOnlyList<TenantUsageHistoryDto> UsageHistory { get; init; } = [];
}

public record TenantInfoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string CompanyType { get; init; } = string.Empty;
    public string? ClinicType { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string? ContactPhone { get; init; }
    public string? Country { get; init; }
    public string? Address { get; init; }
    public DateTime? TrialStartDate { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public bool IsTrialExpired { get; init; }
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }
    public bool IsDatabaseProvisioned { get; init; }
    public DateTime? DatabaseProvisionedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record TenantAdminDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TenantSubscriptionDto
{
    public Guid Id { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string BillingCycle { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public bool AutoRenew { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TenantHealthHistoryDto
{
    public DateTime CheckedAt { get; init; }
    public string OverallStatus { get; init; } = string.Empty;
    public string DatabaseStatus { get; init; } = string.Empty;
    public int? DatabaseLatencyMs { get; init; }
    public string? DatabaseError { get; init; }
}

public record TenantUsageHistoryDto
{
    public DateTime SnapshotDate { get; init; }
    public int ActiveUsers { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveBranches { get; init; }
    public long ApiCallsCount { get; init; }
    public int PatientsCount { get; init; }
    public int AppointmentsCount { get; init; }
}

#endregion

#region Tenant Operations

public record SuspendTenantRequest
{
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; init; }
}

public record ExtendTrialRequest
{
    [Range(1, 90, ErrorMessage = "Trial extension must be between 1 and 90 days")]
    public int Days { get; init; } = 14;

    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; init; }
}

public record ExtendTrialResponse
{
    public DateTime NewTrialEndDate { get; init; }
}

#endregion

#region Usage

public record TenantUsageQuery
{
    [Range(1, 365, ErrorMessage = "Days must be between 1 and 365")]
    public int Days { get; init; } = 30;
}

public record TenantUsageDto
{
    public Guid TenantId { get; init; }
    public UsagePeriodDto Period { get; init; } = null!;
    public UsageSummaryDto Summary { get; init; } = null!;
    public IReadOnlyList<DailyUsageDto> History { get; init; } = [];
}

public record UsagePeriodDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int Days { get; init; }
}

public record UsageSummaryDto
{
    public int CurrentUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public long TotalApiCalls { get; init; }
    public long TotalApiErrors { get; init; }
    public double AvgDailyActiveUsers { get; init; }
}

public record DailyUsageDto
{
    public string Date { get; init; } = string.Empty;
    public int ActiveUsers { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveBranches { get; init; }
    public long ApiCallsCount { get; init; }
    public int PatientsCount { get; init; }
    public int AppointmentsCount { get; init; }
}

public record UsageReportRequest
{
    public string? SnapshotType { get; init; }
    public int ActiveUsers { get; init; }
    public int TotalUsers { get; init; }
    public int NewUsersCount { get; init; }
    public int ActiveBranches { get; init; }
    public long ApiCallsCount { get; init; }
    public long ApiErrorsCount { get; init; }
    public long StorageUsedBytes { get; init; }
    public int DocumentsCount { get; init; }
    public int PatientsCount { get; init; }
    public int AppointmentsCount { get; init; }
    public int InvoicesCount { get; init; }
    public int TotalSessions { get; init; }
    public double AvgSessionDurationMinutes { get; init; }
}

public record UsageReportResponse
{
    public Guid SnapshotId { get; init; }
}

public record CurrentUsageDto
{
    public int ActiveUsers { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveBranches { get; init; }
    public int PatientsCount { get; init; }
    public int AppointmentsCount { get; init; }
    public int InvoicesCount { get; init; }
    public int DocumentsCount { get; init; }
    public long StorageUsedBytes { get; init; }
}

#endregion

#region License

public record LicenseInfoDto
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool CanOperate { get; init; }
    public bool IsTrial { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public LicenseGuardrailsDto License { get; init; } = null!;
    public LicenseSubscriptionDto? Subscription { get; init; }
    public DateTime CheckedAt { get; init; }
}

public record LicenseGuardrailsDto
{
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }
    public bool CanAddBranch { get; init; }
    public bool CanAddUser { get; init; }
    public int RemainingBranches { get; init; }
    public int RemainingUsers { get; init; }
    public double BranchUsagePercent { get; init; }
    public double UserUsagePercent { get; init; }
}

public record LicenseSubscriptionDto
{
    public string Plan { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public int? DaysRemaining { get; init; }
    public bool AutoRenew { get; init; }
}

public record UsageUpdateRequest
{
    [Range(0, 1000, ErrorMessage = "CurrentBranches must be between 0 and 1000")]
    public int? CurrentBranches { get; init; }

    [Range(0, 10000, ErrorMessage = "CurrentUsers must be between 0 and 10000")]
    public int? CurrentUsers { get; init; }
}

public record UsageUpdateResponse
{
    public int CurrentBranches { get; init; }
    public int CurrentUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxUsers { get; init; }
}

#endregion
