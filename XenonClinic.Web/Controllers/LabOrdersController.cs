using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class LabOrdersController : Controller
{
    private readonly ILabService _labService;
    private readonly IBranchScopedService _branchService;
    private readonly ClinicDbContext _context;

    public LabOrdersController(
        ILabService labService,
        IBranchScopedService branchService,
        ClinicDbContext context)
    {
        _labService = labService;
        _branchService = branchService;
        _context = context;
    }

    // GET: LabOrders
    public async Task<IActionResult> Index(LabOrderStatus? status)
    {
        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue)
        {
            return RedirectToAction("SelectBranch", "Home");
        }

        IEnumerable<LabOrder> labOrders;

        if (status.HasValue)
        {
            labOrders = await _labService.GetLabOrdersByStatusAsync(branchId.Value, status.Value);
            ViewBag.CurrentStatus = status.Value;
        }
        else
        {
            labOrders = await _labService.GetLabOrdersByBranchIdAsync(branchId.Value);
        }

        // Get status distribution for filter badges
        var statusDistribution = await _labService.GetOrderStatusDistributionAsync(branchId.Value);
        ViewBag.StatusDistribution = statusDistribution;

        return View(labOrders);
    }

    // GET: LabOrders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var labOrder = await _labService.GetLabOrderByIdAsync(id);
        if (labOrder == null)
        {
            return NotFound();
        }

        return View(labOrder);
    }

    // GET: LabOrders/Create
    public async Task<IActionResult> Create(int? patientId)
    {
        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue)
        {
            return RedirectToAction("SelectBranch", "Home");
        }

        await PopulateDropdowns(branchId.Value, patientId);

        var labOrder = new LabOrder
        {
            BranchId = branchId.Value,
            OrderDate = DateTime.Now,
            PatientId = patientId ?? 0,
            CreatedBy = User.Identity?.Name ?? "Unknown"
        };

        return View(labOrder);
    }

    // POST: LabOrders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LabOrder labOrder)
    {
        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue)
        {
            return RedirectToAction("SelectBranch", "Home");
        }

        if (ModelState.IsValid)
        {
            labOrder.BranchId = branchId.Value;
            labOrder.OrderNumber = await _labService.GenerateLabOrderNumberAsync(branchId.Value);
            labOrder.CreatedBy = User.Identity?.Name ?? "Unknown";
            labOrder.CreatedAt = DateTime.UtcNow;

            await _labService.CreateLabOrderAsync(labOrder);

            TempData["Success"] = "Lab order created successfully.";
            return RedirectToAction(nameof(Details), new { id = labOrder.Id });
        }

        await PopulateDropdowns(branchId.Value, labOrder.PatientId);
        return View(labOrder);
    }

    // GET: LabOrders/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var labOrder = await _labService.GetLabOrderByIdAsync(id);
        if (labOrder == null)
        {
            return NotFound();
        }

        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue || labOrder.BranchId != branchId.Value)
        {
            return Forbid();
        }

        await PopulateDropdowns(branchId.Value, labOrder.PatientId);
        return View(labOrder);
    }

    // POST: LabOrders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LabOrder labOrder)
    {
        if (id != labOrder.Id)
        {
            return NotFound();
        }

        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue || labOrder.BranchId != branchId.Value)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            labOrder.LastModifiedBy = User.Identity?.Name;
            labOrder.LastModifiedAt = DateTime.UtcNow;

            await _labService.UpdateLabOrderAsync(labOrder);

            TempData["Success"] = "Lab order updated successfully.";
            return RedirectToAction(nameof(Details), new { id = labOrder.Id });
        }

        await PopulateDropdowns(branchId.Value, labOrder.PatientId);
        return View(labOrder);
    }

    // POST: LabOrders/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, LabOrderStatus status)
    {
        try
        {
            await _labService.UpdateLabOrderStatusAsync(id, status, User.Identity?.Name ?? "Unknown");
            TempData["Success"] = $"Lab order status updated to {status}.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Lab order not found.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: LabOrders/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var labOrder = await _labService.GetLabOrderByIdAsync(id);
        if (labOrder == null)
        {
            return NotFound();
        }

        var branchId = await _branchService.GetCurrentBranchIdAsync();
        if (!branchId.HasValue || labOrder.BranchId != branchId.Value)
        {
            return Forbid();
        }

        return View(labOrder);
    }

    // POST: LabOrders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _labService.DeleteLabOrderAsync(id);
        TempData["Success"] = "Lab order deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int branchId, int? selectedPatientId = null)
    {
        // Get patients for dropdown
        var patients = await _context.Patients
            .Where(p => p.BranchId == branchId && p.IsActive)
            .OrderBy(p => p.FirstName)
            .Select(p => new
            {
                p.Id,
                FullName = p.FirstName + " " + p.LastName + " (" + p.FileNumber + ")"
            })
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "FullName", selectedPatientId);

        // Get external labs
        var externalLabs = await _labService.GetActiveExternalLabsAsync(branchId);
        ViewBag.ExternalLabs = new SelectList(externalLabs, "Id", "Name");

        // Get available lab tests
        var labTests = await _labService.GetActiveLabTestsAsync(branchId);
        ViewBag.LabTests = labTests;
    }
}
