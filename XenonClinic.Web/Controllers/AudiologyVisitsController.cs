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
public class AudiologyVisitsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<AudiologyVisitsController> _logger;

    public AudiologyVisitsController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<AudiologyVisitsController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    // GET: AudiologyVisits
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation("Loading audiology visits list. Page: {PageNumber}, StartDate: {StartDate}, EndDate: {EndDate}",
                pageNumber, startDate?.ToString("yyyy-MM-dd") ?? "all", endDate?.ToString("yyyy-MM-dd") ?? "all");

            const int pageSize = 20;

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var query = _db.AudiologyVisits
                .Where(v => branchIds.Contains(v.BranchId));

            // Filter by date range if provided
            if (startDate.HasValue)
            {
                query = query.Where(v => v.VisitDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1);
                query = query.Where(v => v.VisitDate < endOfDay);
            }

            var visitsQuery = query
                .Include(v => v.Patient)
                .Include(v => v.Branch)
                .OrderByDescending(v => v.VisitDate)
                .Select(v => new
                {
                    v.Id,
                    v.PatientId,
                    PatientName = v.Patient != null ? v.Patient.FullNameEn : "Unknown",
                    PatientEmiratesId = v.Patient != null ? v.Patient.EmiratesId : "",
                    v.VisitDate,
                    v.ChiefComplaint,
                    v.Diagnosis,
                    BranchName = v.Branch != null ? v.Branch.Name : "Unknown"
                });

            var totalCount = await visitsQuery.CountAsync();
            var visits = await visitsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Visits = visits;

            _logger.LogInformation("Loaded page {PageNumber} of {TotalPages} ({VisitCount} total visits)",
                pageNumber, totalPages, totalCount);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audiology visits list");
            throw;
        }
    }

    // GET: AudiologyVisits/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var visit = await _db.AudiologyVisits
                .Include(v => v.Patient)
                .Include(v => v.Branch)
                .Include(v => v.Audiogram)
                .Where(v => branchIds.Contains(v.BranchId))
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
            {
                _logger.LogWarning("Audiology visit {VisitId} not found or access denied", id);
                return NotFound();
            }

            _logger.LogInformation("Viewing audiology visit details: {VisitId}", id);
            return View(visit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audiology visit details: {VisitId}", id);
            throw;
        }
    }

    // GET: AudiologyVisits/Create
    public async Task<IActionResult> Create(int? patientId, int? appointmentId)
    {
        try
        {
            await LoadPatientsDropdown();

            var model = new CreateAudiologyVisitViewModel
            {
                VisitDate = DateTime.Now.Date
            };

            // Pre-fill patient if coming from patient details
            if (patientId.HasValue)
            {
                model.PatientId = patientId.Value;
            }

            // Pre-fill from appointment if provided
            if (appointmentId.HasValue)
            {
                var appointment = await _db.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);

                if (appointment != null)
                {
                    model.PatientId = appointment.PatientId;
                    model.VisitDate = appointment.StartTime.Date;
                }
            }

            ViewBag.AppointmentId = appointmentId;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create audiology visit form");
            throw;
        }
    }

    // POST: AudiologyVisits/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAudiologyVisitViewModel model, int? appointmentId)
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
                    _logger.LogWarning("User attempted to create visit for patient in unauthorized branch");
                    return Forbid();
                }

                var visit = new AudiologyVisit
                {
                    PatientId = model.PatientId,
                    BranchId = patient.BranchId,
                    VisitDate = model.VisitDate,
                    ChiefComplaint = model.ChiefComplaint,
                    Diagnosis = model.Diagnosis,
                    Plan = model.Plan
                };

                _db.AudiologyVisits.Add(visit);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created audiology visit {VisitId} for patient {PatientId}",
                    visit.Id, visit.PatientId);

                TempData["Success"] = "Audiology visit created successfully.";

                // If created from appointment, redirect back to appointment
                if (appointmentId.HasValue)
                {
                    return RedirectToAction("Details", "Appointments", new { id = appointmentId.Value });
                }

                return RedirectToAction(nameof(Details), new { id = visit.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audiology visit");
                ModelState.AddModelError("", "An error occurred while creating the visit.");
            }
        }

        await LoadPatientsDropdown();
        ViewBag.AppointmentId = appointmentId;
        return View(model);
    }

    // GET: AudiologyVisits/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var visit = await _db.AudiologyVisits
                .Where(v => branchIds.Contains(v.BranchId))
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
            {
                _logger.LogWarning("Audiology visit {VisitId} not found or access denied", id);
                return NotFound();
            }

            var model = new EditAudiologyVisitViewModel
            {
                Id = visit.Id,
                PatientId = visit.PatientId,
                BranchId = visit.BranchId,
                VisitDate = visit.VisitDate,
                ChiefComplaint = visit.ChiefComplaint,
                Diagnosis = visit.Diagnosis,
                Plan = visit.Plan
            };

            await LoadPatientsDropdown();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for audiology visit: {VisitId}", id);
            throw;
        }
    }

    // POST: AudiologyVisits/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditAudiologyVisitViewModel model)
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
                var visit = await _db.AudiologyVisits
                    .Where(v => branchIds.Contains(v.BranchId))
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (visit == null)
                {
                    _logger.LogWarning("Audiology visit {VisitId} not found or access denied", id);
                    return NotFound();
                }

                visit.VisitDate = model.VisitDate;
                visit.ChiefComplaint = model.ChiefComplaint;
                visit.Diagnosis = model.Diagnosis;
                visit.Plan = model.Plan;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Updated audiology visit {VisitId}", id);
                TempData["Success"] = "Audiology visit updated successfully.";

                return RedirectToAction(nameof(Details), new { id = visit.Id });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating audiology visit: {VisitId}", id);
                ModelState.AddModelError("", "The visit was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audiology visit: {VisitId}", id);
                ModelState.AddModelError("", "An error occurred while updating the visit.");
            }
        }

        await LoadPatientsDropdown();
        return View(model);
    }

    // POST: AudiologyVisits/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var visit = await _db.AudiologyVisits
                .Include(v => v.Audiogram)
                .Where(v => branchIds.Contains(v.BranchId))
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
            {
                _logger.LogWarning("Audiology visit {VisitId} not found or access denied", id);
                return NotFound();
            }

            // Delete associated audiogram if exists
            if (visit.Audiogram != null)
            {
                _db.Audiograms.Remove(visit.Audiogram);
            }

            _db.AudiologyVisits.Remove(visit);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted audiology visit {VisitId}", id);
            TempData["Success"] = "Audiology visit deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audiology visit: {VisitId}", id);
            TempData["Error"] = "An error occurred while deleting the visit.";
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
