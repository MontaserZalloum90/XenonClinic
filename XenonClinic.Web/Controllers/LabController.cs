using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class LabController : Controller
{
    private readonly ClinicDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LabController(ClinicDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var today = DateTime.UtcNow.Date;

        var viewModel = new LabDashboardViewModel
        {
            TotalOrders = await _context.LabOrders
                .Where(o => o.BranchId == branchId)
                .CountAsync(),

            PendingOrders = await _context.LabOrders
                .Where(o => o.BranchId == branchId && o.Status == LabOrderStatus.Pending)
                .CountAsync(),

            InProgressOrders = await _context.LabOrders
                .Where(o => o.BranchId == branchId && o.Status == LabOrderStatus.InProgress)
                .CountAsync(),

            CompletedOrdersToday = await _context.LabOrders
                .Where(o => o.BranchId == branchId && o.Status == LabOrderStatus.Completed && o.CompletedDate.HasValue && o.CompletedDate.Value.Date == today)
                .CountAsync(),

            UrgentOrders = await _context.LabOrders
                .Where(o => o.BranchId == branchId && o.IsUrgent && o.Status != LabOrderStatus.Completed && o.Status != LabOrderStatus.Cancelled)
                .CountAsync(),

            PendingResults = await _context.LabResults
                .Where(r => r.LabOrder!.BranchId == branchId && (r.Status == LabResultStatus.Pending || r.Status == LabResultStatus.InProgress))
                .CountAsync(),

            AbnormalResults = await _context.LabResults
                .Where(r => r.LabOrder!.BranchId == branchId && r.IsAbnormal && r.Status == LabResultStatus.Completed)
                .CountAsync(),

            TotalTests = await _context.LabTests
                .Where(t => t.BranchId == branchId)
                .CountAsync(),

            ActiveTests = await _context.LabTests
                .Where(t => t.BranchId == branchId && t.IsActive)
                .CountAsync(),

            TotalRevenue = await _context.LabOrders
                .Where(o => o.BranchId == branchId && o.Status == LabOrderStatus.Completed)
                .SumAsync(o => o.TotalAmount),

            RecentOrders = await _context.LabOrders
                .Where(o => o.BranchId == branchId)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new LabOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PatientName = o.Patient!.FullNameEn,
                    PatientId = o.PatientId,
                    ExternalLabName = o.ExternalLab != null ? o.ExternalLab.Name : null,
                    TotalAmount = o.TotalAmount,
                    IsPaid = o.IsPaid,
                    IsUrgent = o.IsUrgent,
                    TestCount = o.Items.Count,
                    CompletedTests = o.Results.Count(r => r.Status == LabResultStatus.Completed),
                    CollectionDate = o.CollectionDate,
                    ExpectedCompletionDate = o.ExpectedCompletionDate
                })
                .ToListAsync(),

            PendingResults = await _context.LabResults
                .Where(r => r.LabOrder!.BranchId == branchId && (r.Status == LabResultStatus.Pending || r.Status == LabResultStatus.InProgress))
                .OrderBy(r => r.CreatedAt)
                .Take(10)
                .Select(r => new LabResultDto
                {
                    Id = r.Id,
                    LabOrderId = r.LabOrderId,
                    OrderNumber = r.LabOrder!.OrderNumber,
                    TestName = r.LabTest!.TestName,
                    Status = r.Status,
                    ResultDate = r.ResultDate,
                    ResultValue = r.ResultValue,
                    Unit = r.Unit,
                    ReferenceRange = r.ReferenceRange,
                    IsAbnormal = r.IsAbnormal,
                    Interpretation = r.Interpretation,
                    PerformedBy = r.PerformedBy,
                    ReviewedBy = r.ReviewedBy,
                    VerifiedBy = r.VerifiedBy
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    // External Labs
    public async Task<IActionResult> ExternalLabs()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var labs = await _context.ExternalLabs
            .Where(l => l.BranchId == branchId)
            .OrderBy(l => l.Name)
            .Select(l => new ExternalLabDto
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code,
                ContactPerson = l.ContactPerson,
                Email = l.Email,
                Phone = l.Phone,
                City = l.City,
                TurnaroundTimeDays = l.TurnaroundTimeDays,
                IsActive = l.IsActive,
                TotalTests = l.LabTests.Count,
                TotalOrders = l.LabOrders.Count
            })
            .ToListAsync();

        return View(labs);
    }

    [HttpGet]
    public IActionResult CreateExternalLab()
    {
        return View(new CreateExternalLabViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateExternalLab(CreateExternalLabViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var lab = new ExternalLab
        {
            Name = model.Name,
            Code = model.Code,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            Phone = model.Phone,
            Mobile = model.Mobile,
            Address = model.Address,
            City = model.City,
            Country = model.Country,
            Website = model.Website,
            LicenseNumber = model.LicenseNumber,
            LicenseExpiryDate = model.LicenseExpiryDate,
            TurnaroundTimeDays = model.TurnaroundTimeDays,
            IsActive = model.IsActive,
            Notes = model.Notes,
            BranchId = branchId,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedDate = DateTime.UtcNow
        };

        _context.ExternalLabs.Add(lab);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ExternalLabs));
    }

    // Lab Tests
    public async Task<IActionResult> Tests(TestCategory? category)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.LabTests
            .Where(t => t.BranchId == branchId);

        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);

        var tests = await query
            .OrderBy(t => t.TestName)
            .Select(t => new LabTestDto
            {
                Id = t.Id,
                TestCode = t.TestCode,
                TestName = t.TestName,
                Description = t.Description,
                Category = t.Category,
                SpecimenType = t.SpecimenType,
                TurnaroundTimeHours = t.TurnaroundTimeHours,
                Price = t.Price,
                Unit = t.Unit,
                IsActive = t.IsActive,
                RequiresFasting = t.RequiresFasting,
                ExternalLabName = t.ExternalLab != null ? t.ExternalLab.Name : null,
                ExternalLabId = t.ExternalLabId
            })
            .ToListAsync();

        return View(tests);
    }

    [HttpGet]
    public async Task<IActionResult> CreateTest()
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        await PopulateExternalLabsDropdown(branchId);
        return View(new CreateLabTestViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTest(CreateLabTestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user1 = await _userManager.GetUserAsync(User);
            var branchId1 = user1?.PrimaryBranchId ?? 0;
            await PopulateExternalLabsDropdown(branchId1);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var test = new LabTest
        {
            TestCode = model.TestCode,
            TestName = model.TestName,
            Description = model.Description,
            Category = model.Category,
            SpecimenType = model.SpecimenType,
            SpecimenVolume = model.SpecimenVolume,
            TurnaroundTimeHours = model.TurnaroundTimeHours,
            Price = model.Price,
            Unit = model.Unit,
            ReferenceRange = model.ReferenceRange,
            Methodology = model.Methodology,
            IsActive = model.IsActive,
            RequiresFasting = model.RequiresFasting,
            PreparationInstructions = model.PreparationInstructions,
            ExternalLabId = model.ExternalLabId,
            BranchId = branchId,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedDate = DateTime.UtcNow
        };

        _context.LabTests.Add(test);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Tests));
    }

    // Lab Orders
    public async Task<IActionResult> Orders(LabOrderStatus? status, int? patientId, DateTime? fromDate, DateTime? toDate, bool? urgentOnly)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.LabOrders
            .Where(o => o.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (patientId.HasValue)
            query = query.Where(o => o.PatientId == patientId.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        if (urgentOnly.HasValue && urgentOnly.Value)
            query = query.Where(o => o.IsUrgent);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new LabOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                PatientName = o.Patient!.FullNameEn,
                PatientId = o.PatientId,
                ExternalLabName = o.ExternalLab != null ? o.ExternalLab.Name : null,
                TotalAmount = o.TotalAmount,
                IsPaid = o.IsPaid,
                IsUrgent = o.IsUrgent,
                TestCount = o.Items.Count,
                CompletedTests = o.Results.Count(r => r.Status == LabResultStatus.Completed),
                CollectionDate = o.CollectionDate,
                ExpectedCompletionDate = o.ExpectedCompletionDate
            })
            .ToListAsync();

        await PopulatePatientsDropdown(branchId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> CreateOrder(int? patientId)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var model = new CreateLabOrderViewModel();
        if (patientId.HasValue)
            model.PatientId = patientId.Value;

        await PopulatePatientsDropdown(branchId);
        await PopulateExternalLabsDropdown(branchId);
        await PopulateTestsDropdown(branchId);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateLabOrderViewModel model)
    {
        if (!ModelState.IsValid || !model.SelectedTestIds.Any())
        {
            var user1 = await _userManager.GetUserAsync(User);
            var branchId1 = user1?.PrimaryBranchId ?? 0;
            await PopulatePatientsDropdown(branchId1);
            await PopulateExternalLabsDropdown(branchId1);
            await PopulateTestsDropdown(branchId1);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var orderNumber = await GenerateLabOrderNumber(branchId);

        var order = new LabOrder
        {
            OrderNumber = orderNumber,
            OrderDate = model.OrderDate,
            Status = LabOrderStatus.Pending,
            PatientId = model.PatientId,
            BranchId = branchId,
            ExternalLabId = model.ExternalLabId,
            ExpectedCompletionDate = model.ExpectedCompletionDate,
            IsUrgent = model.IsUrgent,
            ClinicalNotes = model.ClinicalNotes,
            Notes = model.Notes,
            OrderedBy = user?.Id,
            CreatedBy = user?.Id ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Add test items
        decimal totalAmount = 0;
        foreach (var testId in model.SelectedTestIds)
        {
            var test = await _context.LabTests.FindAsync(testId);
            if (test != null)
            {
                var item = new LabOrderItem
                {
                    LabTestId = testId,
                    TestCode = test.TestCode,
                    TestName = test.TestName,
                    Price = test.Price
                };
                order.Items.Add(item);
                totalAmount += test.Price;

                // Create pending result
                var result = new LabResult
                {
                    LabOrderItemId = item.Id,
                    LabTestId = testId,
                    Status = LabResultStatus.Pending,
                    CreatedBy = user?.Id ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                order.Results.Add(result);
            }
        }

        order.TotalAmount = totalAmount;

        _context.LabOrders.Add(order);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Orders));
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var order = await _context.LabOrders
            .Include(o => o.Patient)
            .Include(o => o.ExternalLab)
            .Include(o => o.Items).ThenInclude(i => i.LabTest)
            .Include(o => o.Results).ThenInclude(r => r.LabTest)
            .FirstOrDefaultAsync(o => o.Id == id && o.BranchId == branchId);

        if (order == null)
            return NotFound();

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> CollectSpecimen(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var order = await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Id == id && o.BranchId == branchId);

        if (order == null)
            return NotFound();

        order.Status = LabOrderStatus.Collected;
        order.CollectionDate = DateTime.UtcNow;
        order.CollectedBy = user?.UserName;
        order.LastModifiedBy = user?.Id;
        order.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(OrderDetails), new { id });
    }

    // Lab Results
    public async Task<IActionResult> Results(LabResultStatus? status, int? orderId)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var query = _context.LabResults
            .Where(r => r.LabOrder!.BranchId == branchId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (orderId.HasValue)
            query = query.Where(r => r.LabOrderId == orderId.Value);

        var results = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new LabResultDto
            {
                Id = r.Id,
                LabOrderId = r.LabOrderId,
                OrderNumber = r.LabOrder!.OrderNumber,
                TestName = r.LabTest!.TestName,
                Status = r.Status,
                ResultDate = r.ResultDate,
                ResultValue = r.ResultValue,
                Unit = r.Unit,
                ReferenceRange = r.ReferenceRange,
                IsAbnormal = r.IsAbnormal,
                Interpretation = r.Interpretation,
                PerformedBy = r.PerformedBy,
                ReviewedBy = r.ReviewedBy,
                VerifiedBy = r.VerifiedBy
            })
            .ToListAsync();

        return View(results);
    }

    [HttpGet]
    public async Task<IActionResult> EnterResult(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var result = await _context.LabResults
            .Include(r => r.LabOrder)
            .Include(r => r.LabTest)
            .FirstOrDefaultAsync(r => r.Id == id && r.LabOrder!.BranchId == branchId);

        if (result == null)
            return NotFound();

        var model = new CreateLabResultViewModel
        {
            LabOrderId = result.LabOrderId,
            LabOrderItemId = result.LabOrderItemId,
            LabTestId = result.LabTestId,
            ReferenceRange = result.LabTest?.ReferenceRange,
            Unit = result.LabTest?.Unit
        };

        ViewBag.Result = result;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EnterResult(int id, CreateLabResultViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var branchId = user?.PrimaryBranchId ?? 0;

        var result = await _context.LabResults
            .Include(r => r.LabOrder)
            .FirstOrDefaultAsync(r => r.Id == id && r.LabOrder!.BranchId == branchId);

        if (result == null)
            return NotFound();

        result.Status = LabResultStatus.Completed;
        result.ResultDate = model.ResultDate;
        result.ResultValue = model.ResultValue;
        result.Unit = model.Unit;
        result.ReferenceRange = model.ReferenceRange;
        result.IsAbnormal = model.IsAbnormal;
        result.Interpretation = model.Interpretation;
        result.Notes = model.Notes;
        result.PerformedBy = user?.UserName;
        result.PerformedDate = DateTime.UtcNow;
        result.LastModifiedBy = user?.Id;
        result.LastModifiedAt = DateTime.UtcNow;

        // Update order status
        var order = result.LabOrder;
        if (order != null)
        {
            var allCompleted = await _context.LabResults
                .Where(r => r.LabOrderId == order.Id)
                .AllAsync(r => r.Status == LabResultStatus.Completed || r.Status == LabResultStatus.Verified);

            if (allCompleted)
            {
                order.Status = LabOrderStatus.Completed;
                order.CompletedDate = DateTime.UtcNow;
            }
            else if (order.Status == LabOrderStatus.Collected || order.Status == LabOrderStatus.Pending)
            {
                order.Status = LabOrderStatus.InProgress;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Results));
    }

    // Helper methods
    private async Task<string> GenerateLabOrderNumber(int branchId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"LAB-{today:yyyyMM}-";

        var lastOrder = await _context.LabOrders
            .Where(o => o.BranchId == branchId && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null)
        {
            var lastNumberStr = lastOrder.OrderNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task PopulateExternalLabsDropdown(int branchId)
    {
        var labs = await _context.ExternalLabs
            .Where(l => l.BranchId == branchId && l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync();

        ViewBag.ExternalLabs = new SelectList(labs, "Id", "Name");
    }

    private async Task PopulatePatientsDropdown(int branchId)
    {
        var patients = await _context.Patients
            .Where(p => p.BranchId == branchId)
            .OrderBy(p => p.FullNameEn)
            .Select(p => new { p.Id, p.FullNameEn })
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "FullNameEn");
    }

    private async Task PopulateTestsDropdown(int branchId)
    {
        var tests = await _context.LabTests
            .Where(t => t.BranchId == branchId && t.IsActive)
            .OrderBy(t => t.TestName)
            .Select(t => new { t.Id, DisplayText = $"{t.TestName} ({t.TestCode}) - {t.Price:N2} AED" })
            .ToListAsync();

        ViewBag.Tests = new SelectList(tests, "Id", "DisplayText");
    }
}
