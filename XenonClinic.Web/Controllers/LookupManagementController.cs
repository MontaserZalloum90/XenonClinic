using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Constants;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Lookups;
using XenonClinic.Core.Interfaces;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize(Roles = RoleConstants.Combined.SuperAndTenantAdmin)]
public class LookupManagementController : Controller
{
    private readonly ILookupService _lookupService;
    private readonly ITenantService _tenantService;

    public LookupManagementController(
        ILookupService lookupService,
        ITenantService tenantService)
    {
        _lookupService = lookupService;
        _tenantService = tenantService;
    }

    /// <summary>
    /// Dashboard showing all available lookup types.
    /// </summary>
    public async Task<IActionResult> Index(int? tenantId)
    {
        var effectiveTenantId = await GetEffectiveTenantIdAsync(tenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId.Value);

        var model = new LookupManagementDashboardViewModel
        {
            TenantId = effectiveTenantId.Value,
            TenantName = tenant?.Name,
            LookupTypes = GetAllLookupTypes()
        };

        // Get counts for each lookup type
        foreach (var lookupType in model.LookupTypes)
        {
            lookupType.Count = await GetLookupCountByTypeAsync(lookupType.TypeName, effectiveTenantId.Value);
        }

        return View(model);
    }

    /// <summary>
    /// List all items for a specific lookup type.
    /// </summary>
    public async Task<IActionResult> List(string type, int? tenantId, bool includeInactive = false)
    {
        var effectiveTenantId = await GetEffectiveTenantIdAsync(tenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId.Value);
        var lookupInfo = GetLookupTypeInfo(type);

        if (lookupInfo == null)
        {
            return NotFound();
        }

        var items = await GetLookupItemsByTypeAsync(type, effectiveTenantId.Value, includeInactive);

        var model = new LookupListViewModel
        {
            LookupType = type,
            LookupTypeName = lookupInfo.DisplayName,
            TenantId = effectiveTenantId.Value,
            TenantName = tenant?.Name,
            IncludeInactive = includeInactive,
            Items = items
        };

        return View(model);
    }

    /// <summary>
    /// Show form to create a new lookup item.
    /// </summary>
    public async Task<IActionResult> Create(string type, int? tenantId)
    {
        var effectiveTenantId = await GetEffectiveTenantIdAsync(tenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId.Value);
        var lookupInfo = GetLookupTypeInfo(type);

        if (lookupInfo == null)
        {
            return NotFound();
        }

        var model = new LookupEditViewModel
        {
            LookupType = type,
            LookupTypeName = lookupInfo.DisplayName,
            TenantId = effectiveTenantId.Value,
            TenantName = tenant?.Name,
            IsActive = true,
            DisplayOrder = 0
        };

        return View("Edit", model);
    }

    /// <summary>
    /// Show form to edit an existing lookup item.
    /// </summary>
    public async Task<IActionResult> Edit(string type, int id, int? tenantId)
    {
        var effectiveTenantId = await GetEffectiveTenantIdAsync(tenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        var tenant = await _tenantService.GetTenantByIdAsync(effectiveTenantId.Value);
        var lookupInfo = GetLookupTypeInfo(type);

        if (lookupInfo == null)
        {
            return NotFound();
        }

        var lookup = await GetLookupByIdAndTypeAsync(type, id);
        if (lookup == null)
        {
            return NotFound();
        }

        var model = new LookupEditViewModel
        {
            Id = lookup.Id,
            LookupType = type,
            LookupTypeName = lookupInfo.DisplayName,
            Name = lookup.Name,
            Description = lookup.Description,
            Code = lookup.Code,
            DisplayOrder = lookup.DisplayOrder,
            IsActive = lookup.IsActive,
            IsSystemDefault = lookup.IsSystemDefault,
            ColorCode = lookup.ColorCode,
            IconClass = lookup.IconClass,
            TenantId = effectiveTenantId.Value,
            TenantName = tenant?.Name
        };

        return View(model);
    }

    /// <summary>
    /// Save a lookup item (create or update).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(LookupEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", model);
        }

        var effectiveTenantId = await GetEffectiveTenantIdAsync(model.TenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        try
        {
            if (model.Id == 0)
            {
                // Create new
                await CreateLookupByTypeAsync(model.LookupType, model, effectiveTenantId.Value);
                TempData["Success"] = $"{model.LookupTypeName} created successfully.";
            }
            else
            {
                // Update existing
                await UpdateLookupByTypeAsync(model.LookupType, model);
                TempData["Success"] = $"{model.LookupTypeName} updated successfully.";
            }

            return RedirectToAction(nameof(List), new { type = model.LookupType, tenantId = effectiveTenantId.Value });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving {model.LookupTypeName}: {ex.Message}");
            return View("Edit", model);
        }
    }

    /// <summary>
    /// Delete a lookup item.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string type, int id, int? tenantId)
    {
        var effectiveTenantId = await GetEffectiveTenantIdAsync(tenantId);
        if (!effectiveTenantId.HasValue)
        {
            return Forbid();
        }

        try
        {
            var canDelete = await CanDeleteLookupByTypeAsync(type, id);
            if (!canDelete)
            {
                TempData["Error"] = "Cannot delete system default lookup values.";
                return RedirectToAction(nameof(List), new { type, tenantId = effectiveTenantId.Value });
            }

            await DeleteLookupByTypeAsync(type, id);
            TempData["Success"] = "Lookup deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting lookup: {ex.Message}";
        }

        return RedirectToAction(nameof(List), new { type, tenantId = effectiveTenantId.Value });
    }

    // ========================================
    // Helper Methods
    // ========================================

    private async Task<int?> GetEffectiveTenantIdAsync(int? requestedTenantId)
    {
        if (await _tenantService.IsSuperAdminAsync() && requestedTenantId.HasValue)
        {
            return requestedTenantId;
        }

        return await _tenantService.GetCurrentTenantIdAsync();
    }

    private List<LookupTypeInfo> GetAllLookupTypes()
    {
        return new List<LookupTypeInfo>
        {
            // Appointment & Scheduling
            new() { TypeName = "AppointmentType", DisplayName = "Appointment Types", Description = "Types of appointments (e.g., Hearing Test, Fitting, Consultation)", Category = "Appointments", IconClass = "bi-calendar-event" },
            new() { TypeName = "AppointmentStatus", DisplayName = "Appointment Statuses", Description = "Status values for appointments (e.g., Booked, Completed, Cancelled)", Category = "Appointments", IconClass = "bi-calendar-check" },

            // Case Management
            new() { TypeName = "CasePriority", DisplayName = "Case Priorities", Description = "Priority levels for cases (e.g., Low, Medium, High, Urgent)", Category = "Case Management", IconClass = "bi-exclamation-triangle" },
            new() { TypeName = "CaseActivityType", DisplayName = "Case Activity Types", Description = "Types of case activities (e.g., Task, Appointment, Phone Call)", Category = "Case Management", IconClass = "bi-list-task" },
            new() { TypeName = "CaseActivityStatus", DisplayName = "Case Activity Statuses", Description = "Status values for case activities (e.g., Pending, In Progress, Completed)", Category = "Case Management", IconClass = "bi-check-circle" },
            new() { TypeName = "CaseNoteType", DisplayName = "Case Note Types", Description = "Types of case notes (e.g., General, Clinical, Administrative)", Category = "Case Management", IconClass = "bi-journal-text" },

            // HR & Employees
            new() { TypeName = "LeaveType", DisplayName = "Leave Types", Description = "Types of employee leave (e.g., Annual, Sick, Emergency)", Category = "HR & Employees", IconClass = "bi-calendar-x" },
            new() { TypeName = "LeaveStatus", DisplayName = "Leave Statuses", Description = "Status values for leave requests (e.g., Pending, Approved, Rejected)", Category = "HR & Employees", IconClass = "bi-check2-square" },
            new() { TypeName = "EmploymentStatus", DisplayName = "Employment Statuses", Description = "Employee status values (e.g., Active, On Leave, Terminated)", Category = "HR & Employees", IconClass = "bi-person-badge" },
            new() { TypeName = "AttendanceStatus", DisplayName = "Attendance Statuses", Description = "Attendance status values (e.g., Present, Absent, Late)", Category = "HR & Employees", IconClass = "bi-clock" },

            // Inventory & Products
            new() { TypeName = "InventoryCategory", DisplayName = "Inventory Categories", Description = "Product categories (e.g., Hearing Aids, Batteries, Accessories)", Category = "Inventory", IconClass = "bi-box-seam" },
            new() { TypeName = "InventoryTransactionType", DisplayName = "Inventory Transaction Types", Description = "Transaction types (e.g., Purchase, Sale, Adjustment, Transfer)", Category = "Inventory", IconClass = "bi-arrow-left-right" },

            // Clinical
            new() { TypeName = "HearingLossType", DisplayName = "Hearing Loss Types", Description = "Types of hearing loss (e.g., Sensorineural, Conductive, Mixed)", Category = "Clinical", IconClass = "bi-ear" },
            new() { TypeName = "SpecimenType", DisplayName = "Specimen Types", Description = "Lab specimen types (e.g., Blood, Urine, Saliva)", Category = "Clinical", IconClass = "bi-clipboard2-pulse" },
            new() { TypeName = "TestCategory", DisplayName = "Test Categories", Description = "Laboratory test categories (e.g., Hematology, Biochemistry)", Category = "Clinical", IconClass = "bi-clipboard-data" },

            // Financial
            new() { TypeName = "PaymentMethod", DisplayName = "Payment Methods", Description = "Payment methods (e.g., Cash, Card, Bank Transfer)", Category = "Financial", IconClass = "bi-credit-card" },
            new() { TypeName = "PaymentStatus", DisplayName = "Payment Statuses", Description = "Payment status values (e.g., Pending, Paid, Refunded)", Category = "Financial", IconClass = "bi-cash-stack" },
            new() { TypeName = "AccountType", DisplayName = "Account Types", Description = "Financial account types (e.g., Asset, Liability, Revenue)", Category = "Financial", IconClass = "bi-bank" },
            new() { TypeName = "ExpenseStatus", DisplayName = "Expense Statuses", Description = "Expense status values (e.g., Pending, Approved, Paid)", Category = "Financial", IconClass = "bi-receipt" },
            new() { TypeName = "TransactionType", DisplayName = "Transaction Types", Description = "Financial transaction types (e.g., Debit, Credit)", Category = "Financial", IconClass = "bi-arrow-down-up" },
            new() { TypeName = "VoucherStatus", DisplayName = "Voucher Statuses", Description = "Voucher status values (e.g., Draft, Approved, Posted)", Category = "Financial", IconClass = "bi-file-earmark-text" },

            // Sales & Procurement
            new() { TypeName = "QuotationStatus", DisplayName = "Quotation Statuses", Description = "Quotation status values (e.g., Draft, Sent, Accepted)", Category = "Sales & Procurement", IconClass = "bi-file-earmark-check" },
            new() { TypeName = "SaleStatus", DisplayName = "Sale Statuses", Description = "Sale status values (e.g., Draft, Confirmed, Completed)", Category = "Sales & Procurement", IconClass = "bi-cart-check" },
            new() { TypeName = "PurchaseOrderStatus", DisplayName = "Purchase Order Statuses", Description = "Purchase order status values (e.g., Draft, Approved, Received)", Category = "Sales & Procurement", IconClass = "bi-bag-check" },
            new() { TypeName = "GoodsReceiptStatus", DisplayName = "Goods Receipt Statuses", Description = "Goods receipt status values (e.g., Draft, Completed)", Category = "Sales & Procurement", IconClass = "bi-box-arrow-in-down" },
            new() { TypeName = "SupplierPaymentStatus", DisplayName = "Supplier Payment Statuses", Description = "Supplier payment status values (e.g., Pending, Paid)", Category = "Sales & Procurement", IconClass = "bi-cash-coin" },

            // Laboratory
            new() { TypeName = "LabOrderStatus", DisplayName = "Lab Order Statuses", Description = "Lab order status values (e.g., Pending, In Progress, Completed)", Category = "Laboratory", IconClass = "bi-clipboard2-pulse-fill" },
            new() { TypeName = "LabResultStatus", DisplayName = "Lab Result Statuses", Description = "Lab result status values (e.g., Pending, Completed, Verified)", Category = "Laboratory", IconClass = "bi-clipboard-check-fill" }
        };
    }

    private LookupTypeInfo? GetLookupTypeInfo(string typeName)
    {
        return GetAllLookupTypes().FirstOrDefault(t => t.TypeName == typeName);
    }

    // Generic helper methods to work with any lookup type
    private async Task<int> GetLookupCountByTypeAsync(string type, int tenantId)
    {
        return type switch
        {
            "AppointmentType" => await _lookupService.GetLookupCountAsync<AppointmentTypeLookup>(tenantId),
            "AppointmentStatus" => await _lookupService.GetLookupCountAsync<AppointmentStatusLookup>(tenantId),
            "CasePriority" => await _lookupService.GetLookupCountAsync<CasePriorityLookup>(tenantId),
            "CaseActivityType" => await _lookupService.GetLookupCountAsync<CaseActivityTypeLookup>(tenantId),
            "CaseActivityStatus" => await _lookupService.GetLookupCountAsync<CaseActivityStatusLookup>(tenantId),
            "CaseNoteType" => await _lookupService.GetLookupCountAsync<CaseNoteTypeLookup>(tenantId),
            "LeaveType" => await _lookupService.GetLookupCountAsync<LeaveTypeLookup>(tenantId),
            "LeaveStatus" => await _lookupService.GetLookupCountAsync<LeaveStatusLookup>(tenantId),
            "EmploymentStatus" => await _lookupService.GetLookupCountAsync<EmploymentStatusLookup>(tenantId),
            "AttendanceStatus" => await _lookupService.GetLookupCountAsync<AttendanceStatusLookup>(tenantId),
            "InventoryCategory" => await _lookupService.GetLookupCountAsync<InventoryCategoryLookup>(tenantId),
            "InventoryTransactionType" => await _lookupService.GetLookupCountAsync<InventoryTransactionTypeLookup>(tenantId),
            "HearingLossType" => await _lookupService.GetLookupCountAsync<HearingLossTypeLookup>(tenantId),
            "SpecimenType" => await _lookupService.GetLookupCountAsync<SpecimenTypeLookup>(tenantId),
            "TestCategory" => await _lookupService.GetLookupCountAsync<TestCategoryLookup>(tenantId),
            "PaymentMethod" => await _lookupService.GetLookupCountAsync<PaymentMethodLookup>(tenantId),
            "PaymentStatus" => await _lookupService.GetLookupCountAsync<PaymentStatusLookup>(tenantId),
            "AccountType" => await _lookupService.GetLookupCountAsync<AccountTypeLookup>(tenantId),
            "ExpenseStatus" => await _lookupService.GetLookupCountAsync<ExpenseStatusLookup>(tenantId),
            "TransactionType" => await _lookupService.GetLookupCountAsync<TransactionTypeLookup>(tenantId),
            "VoucherStatus" => await _lookupService.GetLookupCountAsync<VoucherStatusLookup>(tenantId),
            "QuotationStatus" => await _lookupService.GetLookupCountAsync<QuotationStatusLookup>(tenantId),
            "SaleStatus" => await _lookupService.GetLookupCountAsync<SaleStatusLookup>(tenantId),
            "PurchaseOrderStatus" => await _lookupService.GetLookupCountAsync<PurchaseOrderStatusLookup>(tenantId),
            "GoodsReceiptStatus" => await _lookupService.GetLookupCountAsync<GoodsReceiptStatusLookup>(tenantId),
            "SupplierPaymentStatus" => await _lookupService.GetLookupCountAsync<SupplierPaymentStatusLookup>(tenantId),
            "LabOrderStatus" => await _lookupService.GetLookupCountAsync<LabOrderStatusLookup>(tenantId),
            "LabResultStatus" => await _lookupService.GetLookupCountAsync<LabResultStatusLookup>(tenantId),
            _ => 0
        };
    }

    private async Task<List<LookupItemViewModel>> GetLookupItemsByTypeAsync(string type, int tenantId, bool includeInactive)
    {
        return type switch
        {
            "AppointmentType" => (await _lookupService.GetLookupsAsync<AppointmentTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "AppointmentStatus" => (await _lookupService.GetLookupsAsync<AppointmentStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "CasePriority" => (await _lookupService.GetLookupsAsync<CasePriorityLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "CaseActivityType" => (await _lookupService.GetLookupsAsync<CaseActivityTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "CaseActivityStatus" => (await _lookupService.GetLookupsAsync<CaseActivityStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "CaseNoteType" => (await _lookupService.GetLookupsAsync<CaseNoteTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "LeaveType" => (await _lookupService.GetLookupsAsync<LeaveTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "LeaveStatus" => (await _lookupService.GetLookupsAsync<LeaveStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "EmploymentStatus" => (await _lookupService.GetLookupsAsync<EmploymentStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "AttendanceStatus" => (await _lookupService.GetLookupsAsync<AttendanceStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "InventoryCategory" => (await _lookupService.GetLookupsAsync<InventoryCategoryLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "InventoryTransactionType" => (await _lookupService.GetLookupsAsync<InventoryTransactionTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "HearingLossType" => (await _lookupService.GetLookupsAsync<HearingLossTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "SpecimenType" => (await _lookupService.GetLookupsAsync<SpecimenTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "TestCategory" => (await _lookupService.GetLookupsAsync<TestCategoryLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "PaymentMethod" => (await _lookupService.GetLookupsAsync<PaymentMethodLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "PaymentStatus" => (await _lookupService.GetLookupsAsync<PaymentStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "AccountType" => (await _lookupService.GetLookupsAsync<AccountTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "ExpenseStatus" => (await _lookupService.GetLookupsAsync<ExpenseStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "TransactionType" => (await _lookupService.GetLookupsAsync<TransactionTypeLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "VoucherStatus" => (await _lookupService.GetLookupsAsync<VoucherStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "QuotationStatus" => (await _lookupService.GetLookupsAsync<QuotationStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "SaleStatus" => (await _lookupService.GetLookupsAsync<SaleStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "PurchaseOrderStatus" => (await _lookupService.GetLookupsAsync<PurchaseOrderStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "GoodsReceiptStatus" => (await _lookupService.GetLookupsAsync<GoodsReceiptStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "SupplierPaymentStatus" => (await _lookupService.GetLookupsAsync<SupplierPaymentStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "LabOrderStatus" => (await _lookupService.GetLookupsAsync<LabOrderStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            "LabResultStatus" => (await _lookupService.GetLookupsAsync<LabResultStatusLookup>(tenantId, includeInactive)).Select(MapToViewModel).ToList(),
            _ => new List<LookupItemViewModel>()
        };
    }

    private async Task<SystemLookup?> GetLookupByIdAndTypeAsync(string type, int id)
    {
        return type switch
        {
            "AppointmentType" => await _lookupService.GetLookupByIdAsync<AppointmentTypeLookup>(id),
            "AppointmentStatus" => await _lookupService.GetLookupByIdAsync<AppointmentStatusLookup>(id),
            "CasePriority" => await _lookupService.GetLookupByIdAsync<CasePriorityLookup>(id),
            "CaseActivityType" => await _lookupService.GetLookupByIdAsync<CaseActivityTypeLookup>(id),
            "CaseActivityStatus" => await _lookupService.GetLookupByIdAsync<CaseActivityStatusLookup>(id),
            "CaseNoteType" => await _lookupService.GetLookupByIdAsync<CaseNoteTypeLookup>(id),
            "LeaveType" => await _lookupService.GetLookupByIdAsync<LeaveTypeLookup>(id),
            "LeaveStatus" => await _lookupService.GetLookupByIdAsync<LeaveStatusLookup>(id),
            "EmploymentStatus" => await _lookupService.GetLookupByIdAsync<EmploymentStatusLookup>(id),
            "AttendanceStatus" => await _lookupService.GetLookupByIdAsync<AttendanceStatusLookup>(id),
            "InventoryCategory" => await _lookupService.GetLookupByIdAsync<InventoryCategoryLookup>(id),
            "InventoryTransactionType" => await _lookupService.GetLookupByIdAsync<InventoryTransactionTypeLookup>(id),
            "HearingLossType" => await _lookupService.GetLookupByIdAsync<HearingLossTypeLookup>(id),
            "SpecimenType" => await _lookupService.GetLookupByIdAsync<SpecimenTypeLookup>(id),
            "TestCategory" => await _lookupService.GetLookupByIdAsync<TestCategoryLookup>(id),
            "PaymentMethod" => await _lookupService.GetLookupByIdAsync<PaymentMethodLookup>(id),
            "PaymentStatus" => await _lookupService.GetLookupByIdAsync<PaymentStatusLookup>(id),
            "AccountType" => await _lookupService.GetLookupByIdAsync<AccountTypeLookup>(id),
            "ExpenseStatus" => await _lookupService.GetLookupByIdAsync<ExpenseStatusLookup>(id),
            "TransactionType" => await _lookupService.GetLookupByIdAsync<TransactionTypeLookup>(id),
            "VoucherStatus" => await _lookupService.GetLookupByIdAsync<VoucherStatusLookup>(id),
            "QuotationStatus" => await _lookupService.GetLookupByIdAsync<QuotationStatusLookup>(id),
            "SaleStatus" => await _lookupService.GetLookupByIdAsync<SaleStatusLookup>(id),
            "PurchaseOrderStatus" => await _lookupService.GetLookupByIdAsync<PurchaseOrderStatusLookup>(id),
            "GoodsReceiptStatus" => await _lookupService.GetLookupByIdAsync<GoodsReceiptStatusLookup>(id),
            "SupplierPaymentStatus" => await _lookupService.GetLookupByIdAsync<SupplierPaymentStatusLookup>(id),
            "LabOrderStatus" => await _lookupService.GetLookupByIdAsync<LabOrderStatusLookup>(id),
            "LabResultStatus" => await _lookupService.GetLookupByIdAsync<LabResultStatusLookup>(id),
            _ => null
        };
    }

    private async Task CreateLookupByTypeAsync(string type, LookupEditViewModel model, int tenantId)
    {
        var lookup = CreateLookupInstance(type);
        MapFromViewModel(model, lookup, tenantId);

        await (type switch
        {
            "AppointmentType" => _lookupService.CreateLookupAsync((AppointmentTypeLookup)lookup),
            "AppointmentStatus" => _lookupService.CreateLookupAsync((AppointmentStatusLookup)lookup),
            "CasePriority" => _lookupService.CreateLookupAsync((CasePriorityLookup)lookup),
            "CaseActivityType" => _lookupService.CreateLookupAsync((CaseActivityTypeLookup)lookup),
            "CaseActivityStatus" => _lookupService.CreateLookupAsync((CaseActivityStatusLookup)lookup),
            "CaseNoteType" => _lookupService.CreateLookupAsync((CaseNoteTypeLookup)lookup),
            "LeaveType" => _lookupService.CreateLookupAsync((LeaveTypeLookup)lookup),
            "LeaveStatus" => _lookupService.CreateLookupAsync((LeaveStatusLookup)lookup),
            "EmploymentStatus" => _lookupService.CreateLookupAsync((EmploymentStatusLookup)lookup),
            "AttendanceStatus" => _lookupService.CreateLookupAsync((AttendanceStatusLookup)lookup),
            "InventoryCategory" => _lookupService.CreateLookupAsync((InventoryCategoryLookup)lookup),
            "InventoryTransactionType" => _lookupService.CreateLookupAsync((InventoryTransactionTypeLookup)lookup),
            "HearingLossType" => _lookupService.CreateLookupAsync((HearingLossTypeLookup)lookup),
            "SpecimenType" => _lookupService.CreateLookupAsync((SpecimenTypeLookup)lookup),
            "TestCategory" => _lookupService.CreateLookupAsync((TestCategoryLookup)lookup),
            "PaymentMethod" => _lookupService.CreateLookupAsync((PaymentMethodLookup)lookup),
            "PaymentStatus" => _lookupService.CreateLookupAsync((PaymentStatusLookup)lookup),
            "AccountType" => _lookupService.CreateLookupAsync((AccountTypeLookup)lookup),
            "ExpenseStatus" => _lookupService.CreateLookupAsync((ExpenseStatusLookup)lookup),
            "TransactionType" => _lookupService.CreateLookupAsync((TransactionTypeLookup)lookup),
            "VoucherStatus" => _lookupService.CreateLookupAsync((VoucherStatusLookup)lookup),
            "QuotationStatus" => _lookupService.CreateLookupAsync((QuotationStatusLookup)lookup),
            "SaleStatus" => _lookupService.CreateLookupAsync((SaleStatusLookup)lookup),
            "PurchaseOrderStatus" => _lookupService.CreateLookupAsync((PurchaseOrderStatusLookup)lookup),
            "GoodsReceiptStatus" => _lookupService.CreateLookupAsync((GoodsReceiptStatusLookup)lookup),
            "SupplierPaymentStatus" => _lookupService.CreateLookupAsync((SupplierPaymentStatusLookup)lookup),
            "LabOrderStatus" => _lookupService.CreateLookupAsync((LabOrderStatusLookup)lookup),
            "LabResultStatus" => _lookupService.CreateLookupAsync((LabResultStatusLookup)lookup),
            _ => Task.FromResult<SystemLookup>(lookup)
        });
    }

    private async Task UpdateLookupByTypeAsync(string type, LookupEditViewModel model)
    {
        var lookup = await GetLookupByIdAndTypeAsync(type, model.Id);
        if (lookup == null) throw new InvalidOperationException("Lookup not found");

        MapFromViewModel(model, lookup, lookup.TenantId ?? 0);

        await (type switch
        {
            "AppointmentType" => _lookupService.UpdateLookupAsync((AppointmentTypeLookup)lookup),
            "AppointmentStatus" => _lookupService.UpdateLookupAsync((AppointmentStatusLookup)lookup),
            "CasePriority" => _lookupService.UpdateLookupAsync((CasePriorityLookup)lookup),
            "CaseActivityType" => _lookupService.UpdateLookupAsync((CaseActivityTypeLookup)lookup),
            "CaseActivityStatus" => _lookupService.UpdateLookupAsync((CaseActivityStatusLookup)lookup),
            "CaseNoteType" => _lookupService.UpdateLookupAsync((CaseNoteTypeLookup)lookup),
            "LeaveType" => _lookupService.UpdateLookupAsync((LeaveTypeLookup)lookup),
            "LeaveStatus" => _lookupService.UpdateLookupAsync((LeaveStatusLookup)lookup),
            "EmploymentStatus" => _lookupService.UpdateLookupAsync((EmploymentStatusLookup)lookup),
            "AttendanceStatus" => _lookupService.UpdateLookupAsync((AttendanceStatusLookup)lookup),
            "InventoryCategory" => _lookupService.UpdateLookupAsync((InventoryCategoryLookup)lookup),
            "InventoryTransactionType" => _lookupService.UpdateLookupAsync((InventoryTransactionTypeLookup)lookup),
            "HearingLossType" => _lookupService.UpdateLookupAsync((HearingLossTypeLookup)lookup),
            "SpecimenType" => _lookupService.UpdateLookupAsync((SpecimenTypeLookup)lookup),
            "TestCategory" => _lookupService.UpdateLookupAsync((TestCategoryLookup)lookup),
            "PaymentMethod" => _lookupService.UpdateLookupAsync((PaymentMethodLookup)lookup),
            "PaymentStatus" => _lookupService.UpdateLookupAsync((PaymentStatusLookup)lookup),
            "AccountType" => _lookupService.UpdateLookupAsync((AccountTypeLookup)lookup),
            "ExpenseStatus" => _lookupService.UpdateLookupAsync((ExpenseStatusLookup)lookup),
            "TransactionType" => _lookupService.UpdateLookupAsync((TransactionTypeLookup)lookup),
            "VoucherStatus" => _lookupService.UpdateLookupAsync((VoucherStatusLookup)lookup),
            "QuotationStatus" => _lookupService.UpdateLookupAsync((QuotationStatusLookup)lookup),
            "SaleStatus" => _lookupService.UpdateLookupAsync((SaleStatusLookup)lookup),
            "PurchaseOrderStatus" => _lookupService.UpdateLookupAsync((PurchaseOrderStatusLookup)lookup),
            "GoodsReceiptStatus" => _lookupService.UpdateLookupAsync((GoodsReceiptStatusLookup)lookup),
            "SupplierPaymentStatus" => _lookupService.UpdateLookupAsync((SupplierPaymentStatusLookup)lookup),
            "LabOrderStatus" => _lookupService.UpdateLookupAsync((LabOrderStatusLookup)lookup),
            "LabResultStatus" => _lookupService.UpdateLookupAsync((LabResultStatusLookup)lookup),
            _ => Task.FromResult<SystemLookup>(lookup)
        });
    }

    private async Task<bool> CanDeleteLookupByTypeAsync(string type, int id)
    {
        return type switch
        {
            "AppointmentType" => await _lookupService.CanDeleteLookupAsync<AppointmentTypeLookup>(id),
            "AppointmentStatus" => await _lookupService.CanDeleteLookupAsync<AppointmentStatusLookup>(id),
            "CasePriority" => await _lookupService.CanDeleteLookupAsync<CasePriorityLookup>(id),
            "CaseActivityType" => await _lookupService.CanDeleteLookupAsync<CaseActivityTypeLookup>(id),
            "CaseActivityStatus" => await _lookupService.CanDeleteLookupAsync<CaseActivityStatusLookup>(id),
            "CaseNoteType" => await _lookupService.CanDeleteLookupAsync<CaseNoteTypeLookup>(id),
            "LeaveType" => await _lookupService.CanDeleteLookupAsync<LeaveTypeLookup>(id),
            "LeaveStatus" => await _lookupService.CanDeleteLookupAsync<LeaveStatusLookup>(id),
            "EmploymentStatus" => await _lookupService.CanDeleteLookupAsync<EmploymentStatusLookup>(id),
            "AttendanceStatus" => await _lookupService.CanDeleteLookupAsync<AttendanceStatusLookup>(id),
            "InventoryCategory" => await _lookupService.CanDeleteLookupAsync<InventoryCategoryLookup>(id),
            "InventoryTransactionType" => await _lookupService.CanDeleteLookupAsync<InventoryTransactionTypeLookup>(id),
            "HearingLossType" => await _lookupService.CanDeleteLookupAsync<HearingLossTypeLookup>(id),
            "SpecimenType" => await _lookupService.CanDeleteLookupAsync<SpecimenTypeLookup>(id),
            "TestCategory" => await _lookupService.CanDeleteLookupAsync<TestCategoryLookup>(id),
            "PaymentMethod" => await _lookupService.CanDeleteLookupAsync<PaymentMethodLookup>(id),
            "PaymentStatus" => await _lookupService.CanDeleteLookupAsync<PaymentStatusLookup>(id),
            "AccountType" => await _lookupService.CanDeleteLookupAsync<AccountTypeLookup>(id),
            "ExpenseStatus" => await _lookupService.CanDeleteLookupAsync<ExpenseStatusLookup>(id),
            "TransactionType" => await _lookupService.CanDeleteLookupAsync<TransactionTypeLookup>(id),
            "VoucherStatus" => await _lookupService.CanDeleteLookupAsync<VoucherStatusLookup>(id),
            "QuotationStatus" => await _lookupService.CanDeleteLookupAsync<QuotationStatusLookup>(id),
            "SaleStatus" => await _lookupService.CanDeleteLookupAsync<SaleStatusLookup>(id),
            "PurchaseOrderStatus" => await _lookupService.CanDeleteLookupAsync<PurchaseOrderStatusLookup>(id),
            "GoodsReceiptStatus" => await _lookupService.CanDeleteLookupAsync<GoodsReceiptStatusLookup>(id),
            "SupplierPaymentStatus" => await _lookupService.CanDeleteLookupAsync<SupplierPaymentStatusLookup>(id),
            "LabOrderStatus" => await _lookupService.CanDeleteLookupAsync<LabOrderStatusLookup>(id),
            "LabResultStatus" => await _lookupService.CanDeleteLookupAsync<LabResultStatusLookup>(id),
            _ => false
        };
    }

    private async Task DeleteLookupByTypeAsync(string type, int id)
    {
        await (type switch
        {
            "AppointmentType" => _lookupService.DeleteLookupAsync<AppointmentTypeLookup>(id),
            "AppointmentStatus" => _lookupService.DeleteLookupAsync<AppointmentStatusLookup>(id),
            "CasePriority" => _lookupService.DeleteLookupAsync<CasePriorityLookup>(id),
            "CaseActivityType" => _lookupService.DeleteLookupAsync<CaseActivityTypeLookup>(id),
            "CaseActivityStatus" => _lookupService.DeleteLookupAsync<CaseActivityStatusLookup>(id),
            "CaseNoteType" => _lookupService.DeleteLookupAsync<CaseNoteTypeLookup>(id),
            "LeaveType" => _lookupService.DeleteLookupAsync<LeaveTypeLookup>(id),
            "LeaveStatus" => _lookupService.DeleteLookupAsync<LeaveStatusLookup>(id),
            "EmploymentStatus" => _lookupService.DeleteLookupAsync<EmploymentStatusLookup>(id),
            "AttendanceStatus" => _lookupService.DeleteLookupAsync<AttendanceStatusLookup>(id),
            "InventoryCategory" => _lookupService.DeleteLookupAsync<InventoryCategoryLookup>(id),
            "InventoryTransactionType" => _lookupService.DeleteLookupAsync<InventoryTransactionTypeLookup>(id),
            "HearingLossType" => _lookupService.DeleteLookupAsync<HearingLossTypeLookup>(id),
            "SpecimenType" => _lookupService.DeleteLookupAsync<SpecimenTypeLookup>(id),
            "TestCategory" => _lookupService.DeleteLookupAsync<TestCategoryLookup>(id),
            "PaymentMethod" => _lookupService.DeleteLookupAsync<PaymentMethodLookup>(id),
            "PaymentStatus" => _lookupService.DeleteLookupAsync<PaymentStatusLookup>(id),
            "AccountType" => _lookupService.DeleteLookupAsync<AccountTypeLookup>(id),
            "ExpenseStatus" => _lookupService.DeleteLookupAsync<ExpenseStatusLookup>(id),
            "TransactionType" => _lookupService.DeleteLookupAsync<TransactionTypeLookup>(id),
            "VoucherStatus" => _lookupService.DeleteLookupAsync<VoucherStatusLookup>(id),
            "QuotationStatus" => _lookupService.DeleteLookupAsync<QuotationStatusLookup>(id),
            "SaleStatus" => _lookupService.DeleteLookupAsync<SaleStatusLookup>(id),
            "PurchaseOrderStatus" => _lookupService.DeleteLookupAsync<PurchaseOrderStatusLookup>(id),
            "GoodsReceiptStatus" => _lookupService.DeleteLookupAsync<GoodsReceiptStatusLookup>(id),
            "SupplierPaymentStatus" => _lookupService.DeleteLookupAsync<SupplierPaymentStatusLookup>(id),
            "LabOrderStatus" => _lookupService.DeleteLookupAsync<LabOrderStatusLookup>(id),
            "LabResultStatus" => _lookupService.DeleteLookupAsync<LabResultStatusLookup>(id),
            _ => Task.CompletedTask
        });
    }

    private SystemLookup CreateLookupInstance(string type)
    {
        return type switch
        {
            "AppointmentType" => new AppointmentTypeLookup(),
            "AppointmentStatus" => new AppointmentStatusLookup(),
            "CasePriority" => new CasePriorityLookup(),
            "CaseActivityType" => new CaseActivityTypeLookup(),
            "CaseActivityStatus" => new CaseActivityStatusLookup(),
            "CaseNoteType" => new CaseNoteTypeLookup(),
            "LeaveType" => new LeaveTypeLookup(),
            "LeaveStatus" => new LeaveStatusLookup(),
            "EmploymentStatus" => new EmploymentStatusLookup(),
            "AttendanceStatus" => new AttendanceStatusLookup(),
            "InventoryCategory" => new InventoryCategoryLookup(),
            "InventoryTransactionType" => new InventoryTransactionTypeLookup(),
            "HearingLossType" => new HearingLossTypeLookup(),
            "SpecimenType" => new SpecimenTypeLookup(),
            "TestCategory" => new TestCategoryLookup(),
            "PaymentMethod" => new PaymentMethodLookup(),
            "PaymentStatus" => new PaymentStatusLookup(),
            "AccountType" => new AccountTypeLookup(),
            "ExpenseStatus" => new ExpenseStatusLookup(),
            "TransactionType" => new TransactionTypeLookup(),
            "VoucherStatus" => new VoucherStatusLookup(),
            "QuotationStatus" => new QuotationStatusLookup(),
            "SaleStatus" => new SaleStatusLookup(),
            "PurchaseOrderStatus" => new PurchaseOrderStatusLookup(),
            "GoodsReceiptStatus" => new GoodsReceiptStatusLookup(),
            "SupplierPaymentStatus" => new SupplierPaymentStatusLookup(),
            "LabOrderStatus" => new LabOrderStatusLookup(),
            "LabResultStatus" => new LabResultStatusLookup(),
            _ => throw new ArgumentException($"Unknown lookup type: {type}")
        };
    }

    private LookupItemViewModel MapToViewModel(SystemLookup lookup)
    {
        return new LookupItemViewModel
        {
            Id = lookup.Id,
            Name = lookup.Name,
            Description = lookup.Description,
            Code = lookup.Code,
            DisplayOrder = lookup.DisplayOrder,
            IsActive = lookup.IsActive,
            IsSystemDefault = lookup.IsSystemDefault,
            ColorCode = lookup.ColorCode,
            IconClass = lookup.IconClass,
            CreatedAt = lookup.CreatedAt,
            CanEdit = !lookup.IsSystemDefault,
            CanDelete = !lookup.IsSystemDefault
        };
    }

    private void MapFromViewModel(LookupEditViewModel model, SystemLookup lookup, int tenantId)
    {
        lookup.Name = model.Name;
        lookup.Description = model.Description;
        lookup.Code = model.Code;
        lookup.DisplayOrder = model.DisplayOrder;
        lookup.IsActive = model.IsActive;
        lookup.IsSystemDefault = model.IsSystemDefault;
        lookup.ColorCode = model.ColorCode;
        lookup.IconClass = model.IconClass;
        lookup.TenantId = tenantId;
    }
}
