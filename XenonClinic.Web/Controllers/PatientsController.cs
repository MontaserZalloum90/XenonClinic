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

    // Patient Details/Profile Page
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            _logger.LogInformation("Loading patient details for ID: {PatientId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();

            var patient = await _db.Patients
                .Include(p => p.Branch)
                .Include(p => p.MedicalHistory)
                .Include(p => p.Documents.Where(d => d.IsActive).OrderByDescending(d => d.UploadDate))
                .Include(p => p.Appointments.OrderByDescending(a => a.StartTime).Take(10))
                .Include(p => p.Visits.OrderByDescending(v => v.VisitDate).Take(10))
                    .ThenInclude(v => v.Audiogram)
                .Include(p => p.Devices)
                .Include(p => p.Invoices)
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found: {PatientId}", id);
                return NotFound();
            }

            _logger.LogInformation("Patient details loaded successfully for ID: {PatientId}", id);
            return View(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading patient details");
            throw;
        }
    }

    // Edit Patient - GET
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            _logger.LogInformation("Loading patient for edit: {PatientId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();

            var patient = await _db.Patients
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found for edit: {PatientId}", id);
                return NotFound();
            }

            var viewModel = new CreatePatientViewModel
            {
                BranchId = patient.BranchId,
                EmiratesId = patient.EmiratesId,
                FullNameEn = patient.FullNameEn,
                FullNameAr = patient.FullNameAr,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email,
                HearingLossType = patient.HearingLossType,
                Notes = patient.Notes
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading patient for edit");
            throw;
        }
    }

    // Edit Patient - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreatePatientViewModel model)
    {
        try
        {
            _logger.LogInformation("Updating patient: {PatientId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found for update: {PatientId}", id);
                return NotFound();
            }

            // Check for duplicate Emirates ID (excluding current patient)
            if (await _db.Patients.AnyAsync(p => p.EmiratesId == model.EmiratesId && p.Id != id))
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
                    _logger.LogWarning("User attempted to update patient in unauthorized branch: {BranchId}", model.BranchId);
                    ModelState.AddModelError(nameof(model.BranchId), "You don't have access to the selected branch.");
                }
            }

            if (ModelState.IsValid)
            {
                patient.BranchId = model.BranchId;
                patient.EmiratesId = model.EmiratesId;
                patient.FullNameEn = model.FullNameEn;
                patient.FullNameAr = model.FullNameAr;
                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.PhoneNumber = model.PhoneNumber;
                patient.Email = model.Email;
                patient.HearingLossType = model.HearingLossType;
                patient.Notes = model.Notes;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Patient updated successfully: {PatientId}", id);
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient");
            throw;
        }
    }

    // Delete/Deactivate Patient - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("Attempting to delete patient: {PatientId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Include(p => p.Appointments)
                .Include(p => p.Visits)
                .Include(p => p.Devices)
                .Include(p => p.Invoices)
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found for deletion: {PatientId}", id);
                return NotFound();
            }

            // Check if patient has related records
            var hasRelatedRecords = patient.Appointments.Any() ||
                                   patient.Visits.Any() ||
                                   patient.Devices.Any() ||
                                   patient.Invoices.Any();

            if (hasRelatedRecords)
            {
                _logger.LogWarning("Cannot delete patient with related records: {PatientId}", id);
                TempData["ErrorMessage"] = "Cannot delete patient with existing appointments, visits, devices, or invoices.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Patient deleted successfully: {PatientId}", id);
            TempData["SuccessMessage"] = "Patient deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient");
            TempData["ErrorMessage"] = "An error occurred while deleting the patient.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ========================================
    // Medical History Management
    // ========================================

    // Medical History - GET
    public async Task<IActionResult> MedicalHistory(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Include(p => p.MedicalHistory)
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            ViewBag.PatientId = id;
            ViewBag.PatientName = patient.FullNameEn;

            return View(patient.MedicalHistory ?? new PatientMedicalHistory { PatientId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading medical history");
            throw;
        }
    }

    // Medical History - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MedicalHistory(int id, PatientMedicalHistory model)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Include(p => p.MedicalHistory)
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (patient.MedicalHistory == null)
                {
                    // Create new medical history
                    model.PatientId = id;
                    model.CreatedDate = DateTime.UtcNow;
                    _db.PatientMedicalHistories.Add(model);
                }
                else
                {
                    // Update existing medical history
                    patient.MedicalHistory.ChronicConditions = model.ChronicConditions;
                    patient.MedicalHistory.Allergies = model.Allergies;
                    patient.MedicalHistory.AllergyReactions = model.AllergyReactions;
                    patient.MedicalHistory.CurrentMedications = model.CurrentMedications;
                    patient.MedicalHistory.PastMedicalHistory = model.PastMedicalHistory;
                    patient.MedicalHistory.SurgicalHistory = model.SurgicalHistory;
                    patient.MedicalHistory.FamilyHistory = model.FamilyHistory;
                    patient.MedicalHistory.IsSmoker = model.IsSmoker;
                    patient.MedicalHistory.ConsumesAlcohol = model.ConsumesAlcohol;
                    patient.MedicalHistory.OccupationalExposure = model.OccupationalExposure;
                    patient.MedicalHistory.NoiseExposureHistory = model.NoiseExposureHistory;
                    patient.MedicalHistory.PreviousHearingAids = model.PreviousHearingAids;
                    patient.MedicalHistory.TinnitusHistory = model.TinnitusHistory;
                    patient.MedicalHistory.BalanceProblems = model.BalanceProblems;
                    patient.MedicalHistory.AdditionalNotes = model.AdditionalNotes;
                    patient.MedicalHistory.LastModifiedDate = DateTime.UtcNow;
                    patient.MedicalHistory.LastModifiedBy = User.Identity?.Name;
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("Medical history saved for patient: {PatientId}", id);
                TempData["SuccessMessage"] = "Medical history saved successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.PatientId = id;
            ViewBag.PatientName = patient.FullNameEn;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving medical history");
            throw;
        }
    }

    // ========================================
    // Document Management
    // ========================================

    // Upload Document - GET
    public async Task<IActionResult> UploadDocument(int id)
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();
        var patient = await _db.Patients
            .Where(p => branchIds.Contains(p.BranchId))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return NotFound();
        }

        ViewBag.PatientId = id;
        ViewBag.PatientName = patient.FullNameEn;
        return View();
    }

    // Upload Document - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file, string documentType, string description, DateTime? expiryDate, string tags)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "File size cannot exceed 10MB.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "patients", id.ToString());
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new PatientDocument
            {
                PatientId = id,
                DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
                DocumentType = documentType,
                Description = description,
                FilePath = $"/uploads/patients/{id}/{uniqueFileName}",
                FileName = file.FileName,
                FileExtension = fileExtension,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                UploadDate = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? "Unknown",
                ExpiryDate = expiryDate,
                Tags = tags,
                IsActive = true
            };

            _db.PatientDocuments.Add(document);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Document uploaded for patient: {PatientId}, File: {FileName}", id, file.FileName);
            TempData["SuccessMessage"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            TempData["ErrorMessage"] = "An error occurred while uploading the document.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // Download Document
    public async Task<IActionResult> DownloadDocument(int id)
    {
        try
        {
            var document = await _db.PatientDocuments
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            // Check branch access
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Contains(document.Patient.BranchId))
            {
                return Forbid();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("Document file not found: {FilePath}", filePath);
                TempData["ErrorMessage"] = "Document file not found.";
                return RedirectToAction(nameof(Details), new { id = document.PatientId });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, document.ContentType, document.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document");
            TempData["ErrorMessage"] = "An error occurred while downloading the document.";
            return RedirectToAction(nameof(Index));
        }
    }

    // Delete Document
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var document = await _db.PatientDocuments
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            // Check branch access
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            if (!branchIds.Contains(document.Patient.BranchId))
            {
                return Forbid();
            }

            var patientId = document.PatientId;

            // Soft delete - mark as inactive
            document.IsActive = false;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Document deleted: {DocumentId} for patient: {PatientId}", id, patientId);
            TempData["SuccessMessage"] = "Document deleted successfully.";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            TempData["ErrorMessage"] = "An error occurred while deleting the document.";
            return RedirectToAction(nameof(Index));
        }
    }
}
