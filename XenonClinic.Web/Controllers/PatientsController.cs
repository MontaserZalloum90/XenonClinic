using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<PatientsController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation("Loading patients list. Page: {PageNumber}, Search: {SearchTerm}",
                pageNumber, search ?? "none");

            const int pageSize = 20;

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var query = branchIds.Any()
                ? _db.Patients.Where(p => branchIds.Contains(p.BranchId))
                : _db.Patients;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.FullNameEn.Contains(search) || p.EmiratesId.Contains(search));
            }

            var patientsQuery = query
                .Include(p => p.Branch)
                .OrderByDescending(p => p.Id)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    BranchId = p.BranchId,
                    EmiratesId = p.EmiratesId,
                    FullNameEn = p.FullNameEn,
                    FullNameAr = p.FullNameAr,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email,
                    HearingLossType = p.HearingLossType,
                    Notes = p.Notes,
                    BranchName = p.Branch != null ? p.Branch.Name : null
                });

            var paginatedPatients = await PaginatedList<PatientDto>.CreateAsync(
                patientsQuery, pageNumber, pageSize);

            ViewBag.Search = search;
            _logger.LogInformation("Loaded page {PageNumber} of {TotalPages} ({PatientCount} total patients)",
                pageNumber, paginatedPatients.TotalPages, paginatedPatients.TotalCount);

            return View(paginatedPatients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading patients list");
            throw;
        }
    }

    public async Task<IActionResult> Create()
    {
        var primaryBranchId = await _branchService.GetCurrentUserBranchIdAsync();
        var viewModel = new CreatePatientViewModel
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            BranchId = primaryBranchId ?? 0
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientViewModel model)
    {
        try
        {
            _logger.LogInformation("Creating new patient with Emirates ID: {EmiratesId}", model.EmiratesId);

            if (await _db.Patients.AnyAsync(p => p.EmiratesId == model.EmiratesId))
            {
                _logger.LogWarning("Duplicate Emirates ID attempted: {EmiratesId}", model.EmiratesId);
                ModelState.AddModelError(string.Empty, "A patient already exists with this Emirates ID.");
            }

            // Verify user has access to the selected branch
            if (model.BranchId > 0)
            {
                var hasAccess = await _branchService.HasAccessToBranchAsync(model.BranchId);
                if (!hasAccess)
                {
                    _logger.LogWarning("User attempted to create patient in unauthorized branch: {BranchId}", model.BranchId);
                    ModelState.AddModelError(nameof(model.BranchId), "You don't have access to the selected branch.");
                }
            }

            if (ModelState.IsValid)
            {
                if (model.BranchId == 0)
                {
                    var primaryBranchId = await _branchService.GetCurrentUserBranchIdAsync();
                    model.BranchId = primaryBranchId ?? await _db.Branches.Select(b => b.Id).FirstAsync();
                }

                var patient = new Patient
                {
                    BranchId = model.BranchId,
                    EmiratesId = model.EmiratesId,
                    FullNameEn = model.FullNameEn,
                    FullNameAr = model.FullNameAr,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    HearingLossType = model.HearingLossType,
                    Notes = model.Notes
                };

                _db.Add(patient);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Patient created successfully with ID: {PatientId}", patient.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReadEid()
    {
        try
        {
            _logger.LogInformation("Mock Emirates ID read requested");
            await Task.Delay(350);

            var demo = new
            {
                emiratesId = "784-1988-1234567-1",
                fullNameEn = "Demo Patient",
                fullNameAr = "مريض تجريبي",
                dateOfBirth = DateTime.UtcNow.AddYears(-35).ToString("yyyy-MM-dd"),
                gender = "M",
                nationality = "UAE"
            };

            return Json(demo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Emirates ID");
            return StatusCode(500, new { error = "Error reading Emirates ID" });
        }
    }
}
