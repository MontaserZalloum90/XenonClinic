using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class HearingDevicesController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<HearingDevicesController> _logger;

    public HearingDevicesController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<HearingDevicesController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    // GET: HearingDevices
    public async Task<IActionResult> Index(bool? activeOnly, int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation("Loading hearing devices list. Page: {PageNumber}, ActiveOnly: {ActiveOnly}",
                pageNumber, activeOnly ?? true);

            const int pageSize = 20;

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var query = _db.HearingDevices
                .Where(d => branchIds.Contains(d.BranchId));

            // Filter by active status (default to active only)
            if (activeOnly ?? true)
            {
                query = query.Where(d => d.IsActive);
            }

            var devicesQuery = query
                .Include(d => d.Patient)
                .Include(d => d.Branch)
                .OrderByDescending(d => d.PurchaseDate)
                .Select(d => new
                {
                    d.Id,
                    d.PatientId,
                    PatientName = d.Patient != null ? d.Patient.FullNameEn : "Unknown",
                    PatientEmiratesId = d.Patient != null ? d.Patient.EmiratesId : "",
                    d.ModelName,
                    d.SerialNumber,
                    d.PurchaseDate,
                    d.WarrantyExpiryDate,
                    d.IsActive,
                    BranchName = d.Branch != null ? d.Branch.Name : "Unknown"
                });

            var totalCount = await devicesQuery.CountAsync();
            var devices = await devicesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.ActiveOnly = activeOnly ?? true;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Devices = devices;

            _logger.LogInformation("Loaded page {PageNumber} of {TotalPages} ({DeviceCount} total devices)",
                pageNumber, totalPages, totalCount);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hearing devices list");
            throw;
        }
    }

    // GET: HearingDevices/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var device = await _db.HearingDevices
                .Include(d => d.Patient)
                .Include(d => d.Branch)
                .Where(d => branchIds.Contains(d.BranchId))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                _logger.LogWarning("Hearing device {DeviceId} not found or access denied", id);
                return NotFound();
            }

            _logger.LogInformation("Viewing hearing device details: {DeviceId}", id);
            return View(device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hearing device details: {DeviceId}", id);
            throw;
        }
    }

    // GET: HearingDevices/Create
    public async Task<IActionResult> Create(int? patientId)
    {
        try
        {
            await LoadPatientsDropdown();

            var model = new CreateHearingDeviceViewModel
            {
                PurchaseDate = DateTime.Now.Date,
                IsActive = true
            };

            // Pre-fill patient if coming from patient details
            if (patientId.HasValue)
            {
                model.PatientId = patientId.Value;
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create hearing device form");
            throw;
        }
    }

    // POST: HearingDevices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHearingDeviceViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Get patient's branch
                var patient = await _db.Patients.FindAsync(model.PatientId);
                if (patient == null)
                {
                    ModelState.AddModelError("", "Patient not found.");
                    await LoadPatientsDropdown();
                    return View(model);
                }

                // Verify user has access to patient's branch
                var branchIds = await _branchService.GetUserBranchIdsAsync();
                if (!branchIds.Contains(patient.BranchId))
                {
                    _logger.LogWarning("User attempted to create device for patient in unauthorized branch");
                    return Forbid();
                }

                var device = new HearingDevice
                {
                    PatientId = model.PatientId,
                    BranchId = patient.BranchId,
                    ModelName = model.ModelName,
                    SerialNumber = model.SerialNumber,
                    PurchaseDate = model.PurchaseDate,
                    WarrantyExpiryDate = model.WarrantyExpiryDate,
                    IsActive = model.IsActive,
                    Notes = model.Notes
                };

                _db.HearingDevices.Add(device);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created hearing device {DeviceId} for patient {PatientId}",
                    device.Id, device.PatientId);

                TempData["Success"] = "Hearing device added successfully.";
                return RedirectToAction(nameof(Details), new { id = device.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hearing device");
                ModelState.AddModelError("", "An error occurred while adding the device.");
            }
        }

        await LoadPatientsDropdown();
        return View(model);
    }

    // GET: HearingDevices/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var device = await _db.HearingDevices
                .Where(d => branchIds.Contains(d.BranchId))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                _logger.LogWarning("Hearing device {DeviceId} not found or access denied", id);
                return NotFound();
            }

            var model = new EditHearingDeviceViewModel
            {
                Id = device.Id,
                PatientId = device.PatientId,
                BranchId = device.BranchId,
                ModelName = device.ModelName,
                SerialNumber = device.SerialNumber,
                PurchaseDate = device.PurchaseDate,
                WarrantyExpiryDate = device.WarrantyExpiryDate,
                IsActive = device.IsActive,
                Notes = device.Notes
            };

            await LoadPatientsDropdown();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for hearing device: {DeviceId}", id);
            throw;
        }
    }

    // POST: HearingDevices/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditHearingDeviceViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var branchIds = await _branchService.GetUserBranchIdsAsync();
                var device = await _db.HearingDevices
                    .Where(d => branchIds.Contains(d.BranchId))
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (device == null)
                {
                    _logger.LogWarning("Hearing device {DeviceId} not found or access denied", id);
                    return NotFound();
                }

                device.ModelName = model.ModelName;
                device.SerialNumber = model.SerialNumber;
                device.PurchaseDate = model.PurchaseDate;
                device.WarrantyExpiryDate = model.WarrantyExpiryDate;
                device.IsActive = model.IsActive;
                device.Notes = model.Notes;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Updated hearing device {DeviceId}", id);
                TempData["Success"] = "Hearing device updated successfully.";

                return RedirectToAction(nameof(Details), new { id = device.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating hearing device: {DeviceId}", id);
                ModelState.AddModelError("", "The device was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hearing device: {DeviceId}", id);
                ModelState.AddModelError("", "An error occurred while updating the device.");
            }
        }

        await LoadPatientsDropdown();
        return View(model);
    }

    // POST: HearingDevices/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var device = await _db.HearingDevices
                .Where(d => branchIds.Contains(d.BranchId))
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                _logger.LogWarning("Hearing device {DeviceId} not found or access denied", id);
                return NotFound();
            }

            _db.HearingDevices.Remove(device);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted hearing device {DeviceId}", id);
            TempData["Success"] = "Hearing device deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hearing device: {DeviceId}", id);
            TempData["Error"] = "An error occurred while deleting the device.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // Helper method to load patients dropdown
    private async Task LoadPatientsDropdown()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();
        var patients = await _db.Patients
            .Where(p => branchIds.Contains(p.BranchId))
            .OrderBy(p => p.FullNameEn)
            .Select(p => new
            {
                p.Id,
                DisplayName = p.FullNameEn + " - " + p.EmiratesId
            })
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "DisplayName");
    }
}
