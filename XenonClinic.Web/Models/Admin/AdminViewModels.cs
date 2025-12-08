using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Models.Admin;

// ==================== Dashboard ====================

public class AdminDashboardViewModel
{
    public SystemStatistics? Statistics { get; set; }
    public List<TenantSummaryViewModel> RecentTenants { get; set; } = new();
    public bool IsSuperAdmin { get; set; }
    public TenantStatistics? CurrentTenantStats { get; set; }
}

public class TenantSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int CompanyCount { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

// ==================== Tenant Management ====================

public class TenantListViewModel
{
    public List<TenantItemViewModel> Tenants { get; set; } = new();
    public bool IncludeInactive { get; set; }
}

public class TenantItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public int CompanyCount { get; set; }
    public int BranchCount { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; }
    public string? SubscriptionPlan { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

public class TenantFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public bool IsActive { get; set; } = true;

    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    public int MaxCompanies { get; set; } = 5;
    public int MaxBranchesPerCompany { get; set; } = 10;
    public int MaxUsersPerTenant { get; set; } = 100;

    public bool IsEdit => Id > 0;
}

public class TenantDetailsViewModel
{
    public Tenant Tenant { get; set; } = null!;
    public TenantStatistics Statistics { get; set; } = null!;
    public List<CompanyItemViewModel> Companies { get; set; } = new();
}

// ==================== Company Management ====================

public class CompanyListViewModel
{
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public List<CompanyItemViewModel> Companies { get; set; } = new();
    public bool IncludeInactive { get; set; }
}

public class CompanyItemViewModel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? City { get; set; }
    public int BranchCount { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; }
}

public class CompanyFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int TenantId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TradeLicenseNumber { get; set; }

    [StringLength(50)]
    public string? TaxRegistrationNumber { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; } = "UAE";

    [StringLength(20)]
    public string? PostalCode { get; set; }

    public string? LogoPath { get; set; }

    [Url]
    public string? Website { get; set; }

    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";

    [StringLength(10)]
    public string? Currency { get; set; } = "AED";

    public string? Timezone { get; set; } = "Arabian Standard Time";
    public bool IsActive { get; set; } = true;

    public List<TenantSelectItem> AvailableTenants { get; set; } = new();

    public bool IsEdit => Id > 0;
}

public class TenantSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CompanyDetailsViewModel
{
    public Company Company { get; set; } = null!;
    public CompanyStatistics Statistics { get; set; } = null!;
    public List<BranchItemViewModel> Branches { get; set; } = new();
}

// ==================== Branch Management ====================

public class BranchListViewModel
{
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public List<BranchItemViewModel> Branches { get; set; } = new();
    public bool IncludeInactive { get; set; }
}

public class BranchItemViewModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
}

public class BranchFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public string? Timezone { get; set; } = "Arabian Standard Time";
    public string? Currency { get; set; } = "AED";
    public bool IsActive { get; set; } = true;
    public bool IsMainBranch { get; set; }

    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public string? WorkingDays { get; set; }

    public List<CompanySelectItem> AvailableCompanies { get; set; } = new();

    public bool IsEdit => Id > 0;
}

public class CompanySelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
}

// ==================== User Management ====================

public class UserListViewModel
{
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string? FilterName { get; set; }
    public List<UserItemViewModel> Users { get; set; } = new();
    public bool IncludeInactive { get; set; }
}

public class UserItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? TenantName { get; set; }
    public string? CompanyName { get; set; }
    public string? PrimaryBranchName { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public int? PrimaryBranchId { get; set; }

    public List<string> SelectedRoles { get; set; } = new();
    public List<int> SelectedBranchIds { get; set; } = new();

    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; }

    [StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Compare("Password")]
    public string? ConfirmPassword { get; set; }

    // For dropdowns
    public List<TenantSelectItem> AvailableTenants { get; set; } = new();
    public List<CompanySelectItem> AvailableCompanies { get; set; } = new();
    public List<BranchSelectItem> AvailableBranches { get; set; } = new();
    public List<string> AvailableRoles { get; set; } = new();

    public bool IsEdit => !string.IsNullOrEmpty(Id);
}

public class BranchSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}

// ==================== Settings Management ====================

public class TenantSettingsFormViewModel
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;

    // Regional settings
    public string DefaultLanguage { get; set; } = "en";
    public string DefaultCurrency { get; set; } = "AED";
    public string DefaultTimezone { get; set; } = "Arabian Standard Time";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";

    // Business settings
    public bool EnableMultiCurrency { get; set; }
    public bool EnableMultiLanguage { get; set; } = true;
    public bool RequireApprovalForExpenses { get; set; } = true;
    public decimal ExpenseApprovalThreshold { get; set; } = 1000;

    // Appointment settings
    public int DefaultAppointmentDurationMinutes { get; set; } = 30;
    public bool EnableOnlineBooking { get; set; }
    public bool SendAppointmentReminders { get; set; } = true;
    public int ReminderHoursBeforeAppointment { get; set; } = 24;

    // Invoice settings
    public string InvoicePrefix { get; set; } = "INV";
    public string QuotationPrefix { get; set; } = "QT";
    public string PurchaseOrderPrefix { get; set; } = "PO";
    public string SalePrefix { get; set; } = "SL";
    public int InvoiceStartNumber { get; set; } = 1;
    public decimal DefaultTaxRate { get; set; } = 5;
    public bool ShowTaxOnInvoice { get; set; } = true;

    // Feature flags
    public bool EnableLabModule { get; set; } = true;
    public bool EnableInventoryModule { get; set; } = true;
    public bool EnableHRModule { get; set; } = true;
    public bool EnableFinanceModule { get; set; } = true;
    public bool EnableProcurementModule { get; set; } = true;
    public bool EnableSalesModule { get; set; } = true;
    public bool EnableAnalyticsModule { get; set; } = true;
    public bool EnablePatientPortal { get; set; }

    // Audit settings
    public bool EnableAuditLogging { get; set; } = true;
    public int AuditLogRetentionDays { get; set; } = 365;
}

// ==================== Module Management ====================

public class ModuleListViewModel
{
    public List<ModuleItemViewModel> Modules { get; set; } = new();
    public int TotalModules { get; set; }
    public int EnabledModules { get; set; }
    public int DisabledModules { get; set; }
}

public class ModuleItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsRequired { get; set; }
    public string[]? Dependencies { get; set; }

    // License information
    public string? LicenseKey { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsLicenseValid => !string.IsNullOrEmpty(LicenseKey);
    public bool IsLicenseExpired => LicenseExpiryDate.HasValue && LicenseExpiryDate.Value < DateTime.UtcNow;
    public int? DaysUntilExpiry => LicenseExpiryDate.HasValue
        ? (int?)(LicenseExpiryDate.Value - DateTime.UtcNow).TotalDays
        : null;
}

public class ModuleDetailsViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsRequired { get; set; }
    public string[]? Dependencies { get; set; }

    // License details
    public string? LicenseKey { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsLicenseValid { get; set; }
    public bool IsLicenseExpired { get; set; }
    public int? DaysUntilExpiry { get; set; }

    // Statistics (can be expanded later)
    public int? TotalUsers { get; set; }
    public DateTime? LastUsed { get; set; }
}
