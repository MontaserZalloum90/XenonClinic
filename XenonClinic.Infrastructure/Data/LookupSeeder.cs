using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities.Lookups;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Seeds default lookup values for all lookup tables.
/// These values are marked as system defaults and cannot be deleted through the UI.
/// </summary>
public static class LookupSeeder
{
    public static async Task SeedLookupsAsync(ClinicDbContext context)
    {
        // Seed all lookup types
        await SeedAppointmentTypesAsync(context);
        await SeedAppointmentStatusesAsync(context);
        await SeedPaymentMethodsAsync(context);
        await SeedPaymentStatusesAsync(context);
        await SeedCasePrioritiesAsync(context);
        await SeedCaseActivityTypesAsync(context);
        await SeedCaseActivityStatusesAsync(context);
        await SeedCaseNoteTypesAsync(context);
        await SeedLeaveTypesAsync(context);
        await SeedLeaveStatusesAsync(context);
        await SeedEmploymentStatusesAsync(context);
        await SeedAttendanceStatusesAsync(context);
        await SeedInventoryCategoriesAsync(context);
        await SeedInventoryTransactionTypesAsync(context);
        await SeedHearingLossTypesAsync(context);
        await SeedAccountTypesAsync(context);
        await SeedExpenseStatusesAsync(context);
        await SeedQuotationStatusesAsync(context);
        await SeedSaleStatusesAsync(context);
        await SeedPurchaseOrderStatusesAsync(context);
        await SeedGoodsReceiptStatusesAsync(context);
        await SeedSupplierPaymentStatusesAsync(context);
        await SeedVoucherStatusesAsync(context);
        await SeedTransactionTypesAsync(context);
        await SeedLabOrderStatusesAsync(context);
        await SeedLabResultStatusesAsync(context);
        await SeedSpecimenTypesAsync(context);
        await SeedTestCategoriesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAppointmentTypesAsync(ClinicDbContext context)
    {
        if (await context.AppointmentTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new AppointmentTypeLookup { Name = "Hearing Test", Code = "HEARING_TEST", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-soundwave", ColorCode = "#0d6efd", DefaultDurationMinutes = 60 },
            new AppointmentTypeLookup { Name = "Fitting", Code = "FITTING", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-earbuds", ColorCode = "#198754", DefaultDurationMinutes = 45 },
            new AppointmentTypeLookup { Name = "Consultation", Code = "CONSULTATION", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-chat-dots", ColorCode = "#0dcaf0", DefaultDurationMinutes = 30 },
            new AppointmentTypeLookup { Name = "Repair", Code = "REPAIR", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-tools", ColorCode = "#ffc107", DefaultDurationMinutes = 30 },
            new AppointmentTypeLookup { Name = "Follow-up", Code = "FOLLOWUP", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-arrow-repeat", ColorCode = "#6c757d", DefaultDurationMinutes = 30 }
        };

        await context.AppointmentTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedAppointmentStatusesAsync(ClinicDbContext context)
    {
        if (await context.AppointmentStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new AppointmentStatusLookup { Name = "Booked", Code = "BOOKED", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#0d6efd", IconClass = "bi-calendar-check", ShowInActiveView = true },
            new AppointmentStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true, ShowInActiveView = false },
            new AppointmentStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, ShowInActiveView = false },
            new AppointmentStatusLookup { Name = "No-Show", Code = "NOSHOW", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-exclamation-triangle", IsCancelledStatus = true, ShowInActiveView = false }
        };

        await context.AppointmentStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedPaymentMethodsAsync(ClinicDbContext context)
    {
        if (await context.PaymentMethodLookups.AnyAsync()) return;

        var methods = new[]
        {
            new PaymentMethodLookup { Name = "Cash", Code = "CASH", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-cash", ColorCode = "#198754" },
            new PaymentMethodLookup { Name = "Card", Code = "CARD", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-credit-card", ColorCode = "#0d6efd", RequiresVerification = true },
            new PaymentMethodLookup { Name = "Bank Transfer", Code = "BANK_TRANSFER", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-bank", ColorCode = "#6c757d", RequiresVerification = true },
            new PaymentMethodLookup { Name = "Insurance", Code = "INSURANCE", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-shield-check", ColorCode = "#0dcaf0", RequiresVerification = true },
            new PaymentMethodLookup { Name = "Installment", Code = "INSTALLMENT", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-calendar-range", ColorCode = "#ffc107", SupportsInstallments = true },
            new PaymentMethodLookup { Name = "Cheque", Code = "CHEQUE", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-receipt", ColorCode = "#6c757d", RequiresVerification = true }
        };

        await context.PaymentMethodLookups.AddRangeAsync(methods);
    }

    private static async Task SeedPaymentStatusesAsync(ClinicDbContext context)
    {
        if (await context.PaymentStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new PaymentStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", IsPendingStatus = true, AllowsPayment = true },
            new PaymentStatusLookup { Name = "Partial", Code = "PARTIAL", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-cash-stack", AllowsPayment = true },
            new PaymentStatusLookup { Name = "Paid", Code = "PAID", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsPaidStatus = true, AllowsPayment = false },
            new PaymentStatusLookup { Name = "Refunded", Code = "REFUNDED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-arrow-counterclockwise", AllowsPayment = false },
            new PaymentStatusLookup { Name = "Overdue", Code = "OVERDUE", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-exclamation-triangle", AllowsPayment = true },
            new PaymentStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", AllowsPayment = false }
        };

        await context.PaymentStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedCasePrioritiesAsync(ClinicDbContext context)
    {
        if (await context.CasePriorityLookups.AnyAsync()) return;

        var priorities = new[]
        {
            new CasePriorityLookup { Name = "Low", Code = "LOW", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-chevron-down", PriorityLevel = 1, ExpectedResponseTimeHours = 72 },
            new CasePriorityLookup { Name = "Medium", Code = "MEDIUM", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-dash", PriorityLevel = 2, ExpectedResponseTimeHours = 48 },
            new CasePriorityLookup { Name = "High", Code = "HIGH", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-chevron-up", PriorityLevel = 3, ExpectedResponseTimeHours = 24 },
            new CasePriorityLookup { Name = "Urgent", Code = "URGENT", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-chevron-double-up", PriorityLevel = 4, ExpectedResponseTimeHours = 4 }
        };

        await context.CasePriorityLookups.AddRangeAsync(priorities);
    }

    private static async Task SeedCaseActivityTypesAsync(ClinicDbContext context)
    {
        if (await context.CaseActivityTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new CaseActivityTypeLookup { Name = "Task", Code = "TASK", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-check-square", ColorCode = "#0d6efd", RequiresDueDate = true, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Appointment", Code = "APPOINTMENT", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-calendar-event", ColorCode = "#198754", RequiresDueDate = true, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Test", Code = "TEST", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-clipboard2-pulse", ColorCode = "#0dcaf0", RequiresDueDate = true, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Follow-up", Code = "FOLLOWUP", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-arrow-repeat", ColorCode = "#ffc107", RequiresDueDate = true, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Phone Call", Code = "PHONE_CALL", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-telephone", ColorCode = "#6c757d", RequiresDueDate = false, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Email", Code = "EMAIL", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-envelope", ColorCode = "#6c757d", RequiresDueDate = false, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Consultation", Code = "CONSULTATION", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, IconClass = "bi-chat-dots", ColorCode = "#0d6efd", RequiresDueDate = true, CanAssign = true },
            new CaseActivityTypeLookup { Name = "Review", Code = "REVIEW", DisplayOrder = 7, IsActive = true, IsSystemDefault = true, IconClass = "bi-eye", ColorCode = "#6c757d", RequiresDueDate = true, CanAssign = true }
        };

        await context.CaseActivityTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedCaseActivityStatusesAsync(ClinicDbContext context)
    {
        if (await context.CaseActivityStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new CaseActivityStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", CountsTowardProgress = true },
            new CaseActivityStatusLookup { Name = "In Progress", Code = "IN_PROGRESS", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-arrow-clockwise", CountsTowardProgress = true },
            new CaseActivityStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true, CountsTowardProgress = true },
            new CaseActivityStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, CountsTowardProgress = false },
            new CaseActivityStatusLookup { Name = "Overdue", Code = "OVERDUE", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-exclamation-triangle", CountsTowardProgress = true }
        };

        await context.CaseActivityStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedCaseNoteTypesAsync(ClinicDbContext context)
    {
        if (await context.CaseNoteTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new CaseNoteTypeLookup { Name = "General", Code = "GENERAL", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-journal-text", ColorCode = "#6c757d" },
            new CaseNoteTypeLookup { Name = "Clinical", Code = "CLINICAL", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-file-medical", ColorCode = "#0d6efd", RequiresReview = true },
            new CaseNoteTypeLookup { Name = "Administrative", Code = "ADMINISTRATIVE", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-clipboard", ColorCode = "#0dcaf0" },
            new CaseNoteTypeLookup { Name = "Follow-up", Code = "FOLLOWUP", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-arrow-repeat", ColorCode = "#ffc107" },
            new CaseNoteTypeLookup { Name = "Important", Code = "IMPORTANT", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-exclamation-circle", ColorCode = "#dc3545", RequiresReview = true }
        };

        await context.CaseNoteTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedLeaveTypesAsync(ClinicDbContext context)
    {
        if (await context.LeaveTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new LeaveTypeLookup { Name = "Annual Leave", Code = "ANNUAL", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-calendar-check", ColorCode = "#198754", IsPaid = true, MaxDaysPerYear = 30, MinimumNoticeDays = 3 },
            new LeaveTypeLookup { Name = "Sick Leave", Code = "SICK", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-thermometer", ColorCode = "#dc3545", IsPaid = true, RequiresDocumentation = true },
            new LeaveTypeLookup { Name = "Emergency Leave", Code = "EMERGENCY", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-exclamation-triangle", ColorCode = "#ffc107", IsPaid = true, MaxDaysPerYear = 5 },
            new LeaveTypeLookup { Name = "Maternity Leave", Code = "MATERNITY", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-heart", ColorCode = "#e83e8c", IsPaid = true, RequiresDocumentation = true, MinimumNoticeDays = 30 },
            new LeaveTypeLookup { Name = "Paternity Leave", Code = "PATERNITY", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-heart", ColorCode = "#0d6efd", IsPaid = true, MaxDaysPerYear = 3, RequiresDocumentation = true },
            new LeaveTypeLookup { Name = "Unpaid Leave", Code = "UNPAID", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-calendar-x", ColorCode = "#6c757d", IsPaid = false, MinimumNoticeDays = 7 },
            new LeaveTypeLookup { Name = "Study Leave", Code = "STUDY", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, IconClass = "bi-book", ColorCode = "#0dcaf0", IsPaid = false, RequiresDocumentation = true, MinimumNoticeDays = 14 },
            new LeaveTypeLookup { Name = "Hajj Leave", Code = "HAJJ", DisplayOrder = 7, IsActive = true, IsSystemDefault = true, IconClass = "bi-moon", ColorCode = "#198754", IsPaid = true, MinimumNoticeDays = 30 }
        };

        await context.LeaveTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedLeaveStatusesAsync(ClinicDbContext context)
    {
        if (await context.LeaveStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new LeaveStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", AllowsEditing = true },
            new LeaveStatusLookup { Name = "Approved", Code = "APPROVED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsApprovedStatus = true, AllowsEditing = false },
            new LeaveStatusLookup { Name = "Rejected", Code = "REJECTED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsRejectedStatus = true, AllowsEditing = false },
            new LeaveStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-slash-circle", AllowsEditing = false }
        };

        await context.LeaveStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedEmploymentStatusesAsync(ClinicDbContext context)
    {
        if (await context.EmploymentStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new EmploymentStatusLookup { Name = "Active", Code = "ACTIVE", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsActiveStatus = true, HasSystemAccess = true, AccruesBenefits = true },
            new EmploymentStatusLookup { Name = "On Leave", Code = "ON_LEAVE", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-calendar-x", IsActiveStatus = true, HasSystemAccess = true, AccruesBenefits = true },
            new EmploymentStatusLookup { Name = "Suspended", Code = "SUSPENDED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-pause-circle", IsActiveStatus = false, HasSystemAccess = false, AccruesBenefits = false },
            new EmploymentStatusLookup { Name = "Terminated", Code = "TERMINATED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsActiveStatus = false, HasSystemAccess = false, AccruesBenefits = false },
            new EmploymentStatusLookup { Name = "Resigned", Code = "RESIGNED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-box-arrow-right", IsActiveStatus = false, HasSystemAccess = false, AccruesBenefits = false }
        };

        await context.EmploymentStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedAttendanceStatusesAsync(ClinicDbContext context)
    {
        if (await context.AttendanceStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new AttendanceStatusLookup { Name = "Present", Code = "PRESENT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", CountsAsPresent = true, WorkHoursMultiplier = 1.0m, AffectsSalary = true },
            new AttendanceStatusLookup { Name = "Absent", Code = "ABSENT", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", CountsAsPresent = false, WorkHoursMultiplier = 0m, RequiresJustification = true, AffectsSalary = true },
            new AttendanceStatusLookup { Name = "Late", Code = "LATE", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", CountsAsPresent = true, WorkHoursMultiplier = 1.0m, RequiresJustification = true, AffectsSalary = true },
            new AttendanceStatusLookup { Name = "Half Day", Code = "HALF_DAY", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-calendar2-minus", CountsAsPresent = true, WorkHoursMultiplier = 0.5m, AffectsSalary = true },
            new AttendanceStatusLookup { Name = "On Leave", Code = "ON_LEAVE", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-calendar-x", CountsAsPresent = false, WorkHoursMultiplier = 0m, AffectsSalary = false },
            new AttendanceStatusLookup { Name = "Holiday", Code = "HOLIDAY", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-calendar-event", CountsAsPresent = false, WorkHoursMultiplier = 0m, AffectsSalary = false }
        };

        await context.AttendanceStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedInventoryCategoriesAsync(ClinicDbContext context)
    {
        if (await context.InventoryCategoryLookups.AnyAsync()) return;

        var categories = new[]
        {
            new InventoryCategoryLookup { Name = "Hearing Aids", Code = "HEARING_AIDS", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-earbuds", ColorCode = "#0d6efd", RequiresSerialNumber = true, DefaultWarrantyMonths = 24 },
            new InventoryCategoryLookup { Name = "Batteries", Code = "BATTERIES", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-lightning", ColorCode = "#ffc107", HasExpiryDate = true },
            new InventoryCategoryLookup { Name = "Accessories", Code = "ACCESSORIES", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-bag", ColorCode = "#0dcaf0", DefaultWarrantyMonths = 6 },
            new InventoryCategoryLookup { Name = "Cleaning Supplies", Code = "CLEANING_SUPPLIES", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-droplet", ColorCode = "#198754" },
            new InventoryCategoryLookup { Name = "Ear Molds", Code = "EAR_MOLDS", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-ear", ColorCode = "#e83e8c", RequiresSerialNumber = true },
            new InventoryCategoryLookup { Name = "Testing Equipment", Code = "TESTING_EQUIPMENT", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-soundwave", ColorCode = "#6c757d", RequiresSerialNumber = true, DefaultWarrantyMonths = 36 },
            new InventoryCategoryLookup { Name = "Consumables", Code = "CONSUMABLES", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, IconClass = "bi-box", ColorCode = "#6c757d", HasExpiryDate = true },
            new InventoryCategoryLookup { Name = "Other", Code = "OTHER", DisplayOrder = 7, IsActive = true, IsSystemDefault = true, IconClass = "bi-three-dots", ColorCode = "#6c757d" }
        };

        await context.InventoryCategoryLookups.AddRangeAsync(categories);
    }

    private static async Task SeedInventoryTransactionTypesAsync(ClinicDbContext context)
    {
        if (await context.InventoryTransactionTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new InventoryTransactionTypeLookup { Name = "Purchase", Code = "PURCHASE", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-cart-plus", ColorCode = "#198754", IncreasesStock = true },
            new InventoryTransactionTypeLookup { Name = "Sale", Code = "SALE", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-cart-dash", ColorCode = "#dc3545", IncreasesStock = false },
            new InventoryTransactionTypeLookup { Name = "Adjustment", Code = "ADJUSTMENT", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-arrow-left-right", ColorCode = "#ffc107", RequiresApproval = true, RequiresReason = true },
            new InventoryTransactionTypeLookup { Name = "Transfer", Code = "TRANSFER", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-box-arrow-right", ColorCode = "#0dcaf0", RequiresApproval = true },
            new InventoryTransactionTypeLookup { Name = "Return", Code = "RETURN", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-arrow-counterclockwise", ColorCode = "#6c757d", IncreasesStock = true, RequiresReason = true }
        };

        await context.InventoryTransactionTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedHearingLossTypesAsync(ClinicDbContext context)
    {
        if (await context.HearingLossTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new HearingLossTypeLookup { Name = "Normal", Code = "NORMAL", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-check-circle", ColorCode = "#198754", ClinicalDescription = "No hearing loss detected" },
            new HearingLossTypeLookup { Name = "Sensorineural", Code = "SENSORINEURAL", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-ear", ColorCode = "#dc3545", ClinicalDescription = "Inner ear or auditory nerve damage" },
            new HearingLossTypeLookup { Name = "Conductive", Code = "CONDUCTIVE", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-ear", ColorCode = "#ffc107", ClinicalDescription = "Outer or middle ear problem" },
            new HearingLossTypeLookup { Name = "Mixed", Code = "MIXED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-ear", ColorCode = "#0dcaf0", ClinicalDescription = "Combination of sensorineural and conductive" }
        };

        await context.HearingLossTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedAccountTypesAsync(ClinicDbContext context)
    {
        if (await context.AccountTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new AccountTypeLookup { Name = "Asset", Code = "ASSET", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-cash-coin", ColorCode = "#198754", NormalDebitBalance = true, IsBalanceSheetAccount = true },
            new AccountTypeLookup { Name = "Liability", Code = "LIABILITY", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-credit-card", ColorCode = "#dc3545", NormalDebitBalance = false, IsBalanceSheetAccount = true },
            new AccountTypeLookup { Name = "Equity", Code = "EQUITY", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-pie-chart", ColorCode = "#0d6efd", NormalDebitBalance = false, IsBalanceSheetAccount = true },
            new AccountTypeLookup { Name = "Revenue", Code = "REVENUE", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-graph-up", ColorCode = "#198754", NormalDebitBalance = false, IsBalanceSheetAccount = false },
            new AccountTypeLookup { Name = "Expense", Code = "EXPENSE", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-graph-down", ColorCode = "#dc3545", NormalDebitBalance = true, IsBalanceSheetAccount = false }
        };

        await context.AccountTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedExpenseStatusesAsync(ClinicDbContext context)
    {
        if (await context.ExpenseStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new ExpenseStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", AllowsEditing = true },
            new ExpenseStatusLookup { Name = "Approved", Code = "APPROVED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-check", IsApprovedStatus = true, AllowsEditing = false },
            new ExpenseStatusLookup { Name = "Paid", Code = "PAID", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsApprovedStatus = true, IsPaidStatus = true, AllowsEditing = false },
            new ExpenseStatusLookup { Name = "Rejected", Code = "REJECTED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", AllowsEditing = false }
        };

        await context.ExpenseStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedQuotationStatusesAsync(ClinicDbContext context)
    {
        if (await context.QuotationStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new QuotationStatusLookup { Name = "Draft", Code = "DRAFT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-file-earmark", AllowsEditing = true },
            new QuotationStatusLookup { Name = "Sent", Code = "SENT", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-send", AllowsEditing = false },
            new QuotationStatusLookup { Name = "Accepted", Code = "ACCEPTED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsFinalStatus = true, AllowsEditing = false },
            new QuotationStatusLookup { Name = "Rejected", Code = "REJECTED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsFinalStatus = true, AllowsEditing = false },
            new QuotationStatusLookup { Name = "Expired", Code = "EXPIRED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock-history", IsFinalStatus = true, AllowsEditing = false }
        };

        await context.QuotationStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedSaleStatusesAsync(ClinicDbContext context)
    {
        if (await context.SaleStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new SaleStatusLookup { Name = "Draft", Code = "DRAFT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-file-earmark", AllowsEditing = true },
            new SaleStatusLookup { Name = "Confirmed", Code = "CONFIRMED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-check", AllowsEditing = false },
            new SaleStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true, AllowsEditing = false },
            new SaleStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, AllowsEditing = false },
            new SaleStatusLookup { Name = "Refunded", Code = "REFUNDED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-arrow-counterclockwise", AllowsEditing = false }
        };

        await context.SaleStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedPurchaseOrderStatusesAsync(ClinicDbContext context)
    {
        if (await context.PurchaseOrderStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new PurchaseOrderStatusLookup { Name = "Draft", Code = "DRAFT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-file-earmark", AllowsEditing = true },
            new PurchaseOrderStatusLookup { Name = "Submitted", Code = "SUBMITTED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-send", AllowsEditing = false },
            new PurchaseOrderStatusLookup { Name = "Approved", Code = "APPROVED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check", AllowsEditing = false },
            new PurchaseOrderStatusLookup { Name = "Ordered", Code = "ORDERED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#0d6efd", IconClass = "bi-cart-check", AllowsReceiving = true, AllowsEditing = false },
            new PurchaseOrderStatusLookup { Name = "Partially Received", Code = "PARTIALLY_RECEIVED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-box-seam", AllowsReceiving = true, AllowsEditing = false },
            new PurchaseOrderStatusLookup { Name = "Received", Code = "RECEIVED", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsFinalStatus = true, AllowsEditing = false },
            new PurchaseOrderStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsFinalStatus = true, AllowsEditing = false }
        };

        await context.PurchaseOrderStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedGoodsReceiptStatusesAsync(ClinicDbContext context)
    {
        if (await context.GoodsReceiptStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new GoodsReceiptStatusLookup { Name = "Draft", Code = "DRAFT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-file-earmark", AllowsEditing = true },
            new GoodsReceiptStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true, AllowsEditing = false },
            new GoodsReceiptStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, AllowsEditing = false }
        };

        await context.GoodsReceiptStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedSupplierPaymentStatusesAsync(ClinicDbContext context)
    {
        if (await context.SupplierPaymentStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new SupplierPaymentStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", AllowsEditing = true },
            new SupplierPaymentStatusLookup { Name = "Paid", Code = "PAID", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsPaidStatus = true, AllowsEditing = false },
            new SupplierPaymentStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, AllowsEditing = false }
        };

        await context.SupplierPaymentStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedVoucherStatusesAsync(ClinicDbContext context)
    {
        if (await context.VoucherStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new VoucherStatusLookup { Name = "Draft", Code = "DRAFT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#6c757d", IconClass = "bi-file-earmark", AllowsEditing = true },
            new VoucherStatusLookup { Name = "Approved", Code = "APPROVED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-check", AllowsEditing = false },
            new VoucherStatusLookup { Name = "Posted", Code = "POSTED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsPostedStatus = true, AllowsEditing = false },
            new VoucherStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true, AllowsEditing = false }
        };

        await context.VoucherStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedTransactionTypesAsync(ClinicDbContext context)
    {
        if (await context.TransactionTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new TransactionTypeLookup { Name = "Debit", Code = "DEBIT", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-plus-circle", ColorCode = "#198754", IsDebitTransaction = true, Multiplier = 1 },
            new TransactionTypeLookup { Name = "Credit", Code = "CREDIT", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-dash-circle", ColorCode = "#dc3545", IsDebitTransaction = false, Multiplier = -1 }
        };

        await context.TransactionTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedLabOrderStatusesAsync(ClinicDbContext context)
    {
        if (await context.LabOrderStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new LabOrderStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock" },
            new LabOrderStatusLookup { Name = "Collected", Code = "COLLECTED", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-droplet" },
            new LabOrderStatusLookup { Name = "In Progress", Code = "IN_PROGRESS", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#0d6efd", IconClass = "bi-arrow-clockwise", AllowsResultEntry = true },
            new LabOrderStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true },
            new LabOrderStatusLookup { Name = "Cancelled", Code = "CANCELLED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#dc3545", IconClass = "bi-x-circle", IsCancelledStatus = true }
        };

        await context.LabOrderStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedLabResultStatusesAsync(ClinicDbContext context)
    {
        if (await context.LabResultStatusLookups.AnyAsync()) return;

        var statuses = new[]
        {
            new LabResultStatusLookup { Name = "Pending", Code = "PENDING", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, ColorCode = "#ffc107", IconClass = "bi-clock", AllowsEditing = true },
            new LabResultStatusLookup { Name = "In Progress", Code = "IN_PROGRESS", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, ColorCode = "#0dcaf0", IconClass = "bi-arrow-clockwise", AllowsEditing = true },
            new LabResultStatusLookup { Name = "Completed", Code = "COMPLETED", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check", IsCompletedStatus = true, RequiresVerification = true, AllowsEditing = false },
            new LabResultStatusLookup { Name = "Reviewed", Code = "REVIEWED", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, ColorCode = "#0d6efd", IconClass = "bi-eye", IsCompletedStatus = true, AllowsEditing = false },
            new LabResultStatusLookup { Name = "Verified", Code = "VERIFIED", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, ColorCode = "#198754", IconClass = "bi-check-circle", IsCompletedStatus = true, AllowsEditing = false }
        };

        await context.LabResultStatusLookups.AddRangeAsync(statuses);
    }

    private static async Task SeedSpecimenTypesAsync(ClinicDbContext context)
    {
        if (await context.SpecimenTypeLookups.AnyAsync()) return;

        var types = new[]
        {
            new SpecimenTypeLookup { Name = "Blood", Code = "BLOOD", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-droplet", ColorCode = "#dc3545", ExpiryHours = 48 },
            new SpecimenTypeLookup { Name = "Urine", Code = "URINE", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-cup", ColorCode = "#ffc107", ExpiryHours = 24 },
            new SpecimenTypeLookup { Name = "Saliva", Code = "SALIVA", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-droplet-half", ColorCode = "#0dcaf0", ExpiryHours = 12 },
            new SpecimenTypeLookup { Name = "Tissue", Code = "TISSUE", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-layers", ColorCode = "#e83e8c", ExpiryHours = 72 },
            new SpecimenTypeLookup { Name = "Swab", Code = "SWAB", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-bandaid", ColorCode = "#198754", ExpiryHours = 24 },
            new SpecimenTypeLookup { Name = "Stool", Code = "STOOL", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-inbox", ColorCode = "#6c757d", ExpiryHours = 48 },
            new SpecimenTypeLookup { Name = "Other", Code = "OTHER", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, IconClass = "bi-three-dots", ColorCode = "#6c757d" }
        };

        await context.SpecimenTypeLookups.AddRangeAsync(types);
    }

    private static async Task SeedTestCategoriesAsync(ClinicDbContext context)
    {
        if (await context.TestCategoryLookups.AnyAsync()) return;

        var categories = new[]
        {
            new TestCategoryLookup { Name = "Hematology", Code = "HEMATOLOGY", DisplayOrder = 0, IsActive = true, IsSystemDefault = true, IconClass = "bi-droplet", ColorCode = "#dc3545", DefaultTurnaroundTimeHours = 24 },
            new TestCategoryLookup { Name = "Biochemistry", Code = "BIOCHEMISTRY", DisplayOrder = 1, IsActive = true, IsSystemDefault = true, IconClass = "bi-flask", ColorCode = "#0d6efd", DefaultTurnaroundTimeHours = 48 },
            new TestCategoryLookup { Name = "Microbiology", Code = "MICROBIOLOGY", DisplayOrder = 2, IsActive = true, IsSystemDefault = true, IconClass = "bi-virus", ColorCode = "#198754", DefaultTurnaroundTimeHours = 72 },
            new TestCategoryLookup { Name = "Immunology", Code = "IMMUNOLOGY", DisplayOrder = 3, IsActive = true, IsSystemDefault = true, IconClass = "bi-shield-check", ColorCode = "#0dcaf0", DefaultTurnaroundTimeHours = 48 },
            new TestCategoryLookup { Name = "Pathology", Code = "PATHOLOGY", DisplayOrder = 4, IsActive = true, IsSystemDefault = true, IconClass = "bi-microscope", ColorCode = "#e83e8c", DefaultTurnaroundTimeHours = 96 },
            new TestCategoryLookup { Name = "Imaging", Code = "IMAGING", DisplayOrder = 5, IsActive = true, IsSystemDefault = true, IconClass = "bi-camera", ColorCode = "#6c757d", DefaultTurnaroundTimeHours = 24 },
            new TestCategoryLookup { Name = "Audiology", Code = "AUDIOLOGY", DisplayOrder = 6, IsActive = true, IsSystemDefault = true, IconClass = "bi-ear", ColorCode = "#0d6efd", DefaultTurnaroundTimeHours = 1, RequiresSpecialization = true },
            new TestCategoryLookup { Name = "Cardiology", Code = "CARDIOLOGY", DisplayOrder = 7, IsActive = true, IsSystemDefault = true, IconClass = "bi-heart-pulse", ColorCode = "#dc3545", DefaultTurnaroundTimeHours = 24, RequiresSpecialization = true },
            new TestCategoryLookup { Name = "Other", Code = "OTHER", DisplayOrder = 8, IsActive = true, IsSystemDefault = true, IconClass = "bi-three-dots", ColorCode = "#6c757d" }
        };

        await context.TestCategoryLookups.AddRangeAsync(categories);
    }
}
