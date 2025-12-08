using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<AppointmentsController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    // GET: Appointments
    public async Task<IActionResult> Index(DateTime? date, AppointmentStatus? status, int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation("Loading appointments list. Page: {PageNumber}, Date: {Date}, Status: {Status}",
                pageNumber, date?.ToString("yyyy-MM-dd") ?? "all", status?.ToString() ?? "all");

            const int pageSize = 20;

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var query = branchIds.Any()
                ? _db.Appointments.Where(a => branchIds.Contains(a.BranchId))
                : _db.Appointments;

            // Filter by date if provided
            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(a => a.StartTime >= startOfDay && a.StartTime < endOfDay);
            }

            // Filter by status if provided
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointmentsQuery = query
                .Include(a => a.Patient)
                .Include(a => a.Branch)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.PatientId,
                    PatientName = a.Patient != null ? a.Patient.FullNameEn : "Unknown",
                    PatientEmiratesId = a.Patient != null ? a.Patient.EmiratesId : "",
                    a.StartTime,
                    a.EndTime,
                    a.Type,
                    a.Status,
                    a.Notes,
                    BranchName = a.Branch != null ? a.Branch.Name : "Unknown"
                });

            var totalCount = await appointmentsQuery.CountAsync();
            var appointments = await appointmentsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.CurrentDate = date;
            ViewBag.CurrentStatus = status;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Appointments = appointments;

            _logger.LogInformation("Loaded page {PageNumber} of {TotalPages} ({AppointmentCount} total appointments)",
                pageNumber, totalPages, totalCount);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments list");
            throw;
        }
    }

    // GET: Appointments/Calendar
    public IActionResult Calendar()
    {
        return View();
    }

    // GET: Appointments/GetAppointmentsJson
    [HttpGet]
    public async Task<IActionResult> GetAppointmentsJson(DateTime start, DateTime end, int? providerId)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var query = _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .Where(a => branchIds.Contains(a.BranchId)
                    && a.StartTime >= start
                    && a.StartTime <= end);

            // Filter by provider if specified
            if (providerId.HasValue)
            {
                query = query.Where(a => a.ProviderId == providerId.Value);
            }

            var appointments = await query
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Patient != null ? a.Patient.FullNameEn : "Unknown Patient",
                    start = a.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = a.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    backgroundColor = a.Status switch
                    {
                        AppointmentStatus.Booked => "#0d6efd",      // Blue
                        AppointmentStatus.Completed => "#198754",   // Green
                        AppointmentStatus.Cancelled => "#dc3545",   // Red
                        AppointmentStatus.NoShow => "#ffc107",      // Yellow
                        _ => "#6c757d"                              // Gray
                    },
                    borderColor = a.Status switch
                    {
                        AppointmentStatus.Booked => "#0d6efd",
                        AppointmentStatus.Completed => "#198754",
                        AppointmentStatus.Cancelled => "#dc3545",
                        AppointmentStatus.NoShow => "#ffc107",
                        _ => "#6c757d"
                    },
                    extendedProps = new
                    {
                        patientId = a.PatientId,
                        patientName = a.Patient != null ? a.Patient.FullNameEn : "Unknown",
                        providerId = a.ProviderId,
                        providerName = a.Provider != null ? a.Provider.FullNameEn : null,
                        type = a.Type.ToString(),
                        status = a.Status.ToString(),
                        notes = a.Notes
                    }
                })
                .ToListAsync();

            return Json(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments for calendar");
            return StatusCode(500, new { error = "Error loading appointments" });
        }
    }

    // GET: Appointments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var appointment = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Branch)
                .Include(a => a.Provider)
                .Where(a => branchIds.Contains(a.BranchId))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", id);
                return NotFound();
            }

            return View(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointment details: {AppointmentId}", id);
            throw;
        }
    }

    // GET: Appointments/Create
    public async Task<IActionResult> Create(int? patientId)
    {
        try
        {
            var primaryBranchId = await _branchService.GetCurrentUserBranchIdAsync();
            var viewModel = new CreateAppointmentViewModel
            {
                AppointmentDate = DateTime.Today,
                StartTime = new TimeSpan(9, 0, 0),
                DurationMinutes = 30,
                Type = AppointmentType.Consultation,
                Status = AppointmentStatus.Booked,
                BranchId = primaryBranchId ?? 0
            };

            if (patientId.HasValue)
            {
                viewModel.PatientId = patientId.Value;
            }

            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create appointment form");
            throw;
        }
    }

    // POST: Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel model)
    {
        try
        {
            _logger.LogInformation("Creating new appointment for patient: {PatientId}", model.PatientId);

            // Validate that patient exists and user has access
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var patient = await _db.Patients
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
            {
                ModelState.AddModelError(string.Empty, "Patient not found or you don't have access to this patient.");
            }

            // Check for overlapping appointments
            var startDateTime = model.StartDateTime;
            var endDateTime = model.EndDateTime;

            var hasOverlap = await _db.Appointments
                .Where(a => a.PatientId == model.PatientId
                    && a.Status != AppointmentStatus.Cancelled
                    && a.StartTime < endDateTime
                    && a.EndTime > startDateTime)
                .AnyAsync();

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "This patient already has an appointment during this time. Please choose a different time slot.");
            }

            if (ModelState.IsValid)
            {
                var appointment = new Appointment
                {
                    PatientId = model.PatientId,
                    ProviderId = model.ProviderId,
                    BranchId = patient!.BranchId,
                    StartTime = startDateTime,
                    EndTime = endDateTime,
                    Type = model.Type,
                    Status = model.Status,
                    Notes = model.Notes
                };

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Appointment created successfully: {AppointmentId}", appointment.Id);
                TempData["SuccessMessage"] = "Appointment created successfully.";
                return RedirectToAction(nameof(Details), new { id = appointment.Id });
            }

            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            TempData["ErrorMessage"] = "An error occurred while creating the appointment.";
            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(model);
        }
    }

    // GET: Appointments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var appointment = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => branchIds.Contains(a.BranchId))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", id);
                return NotFound();
            }

            var durationMinutes = (int)(appointment.EndTime - appointment.StartTime).TotalMinutes;

            var viewModel = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                AppointmentDate = appointment.StartTime.Date,
                StartTime = appointment.StartTime.TimeOfDay,
                DurationMinutes = durationMinutes,
                Type = appointment.Type,
                Status = appointment.Status,
                Notes = appointment.Notes,
                BranchId = appointment.BranchId
            };

            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit appointment form: {AppointmentId}", id);
            throw;
        }
    }

    // POST: Appointments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditAppointmentViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        try
        {
            _logger.LogInformation("Updating appointment: {AppointmentId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var appointment = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => branchIds.Contains(a.BranchId))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", id);
                return NotFound();
            }

            // Validate patient exists and user has access
            var patient = await _db.Patients
                .Where(p => branchIds.Contains(p.BranchId))
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
            {
                ModelState.AddModelError(string.Empty, "Patient not found or you don't have access to this patient.");
            }

            // Check for overlapping appointments (excluding current appointment)
            var startDateTime = model.StartDateTime;
            var endDateTime = model.EndDateTime;

            var hasOverlap = await _db.Appointments
                .Where(a => a.Id != id
                    && a.PatientId == model.PatientId
                    && a.Status != AppointmentStatus.Cancelled
                    && a.StartTime < endDateTime
                    && a.EndTime > startDateTime)
                .AnyAsync();

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "This patient already has an appointment during this time. Please choose a different time slot.");
            }

            if (ModelState.IsValid)
            {
                appointment.PatientId = model.PatientId;
                appointment.ProviderId = model.ProviderId;
                appointment.BranchId = patient!.BranchId;
                appointment.StartTime = startDateTime;
                appointment.EndTime = endDateTime;
                appointment.Type = model.Type;
                appointment.Status = model.Status;
                appointment.Notes = model.Notes;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Appointment updated successfully: {AppointmentId}", id);
                TempData["SuccessMessage"] = "Appointment updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment: {AppointmentId}", id);
            TempData["ErrorMessage"] = "An error occurred while updating the appointment.";
            await LoadPatientsDropdown();
            await LoadProvidersDropdown();
            return View(model);
        }
    }

    // POST: Appointments/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            _logger.LogInformation("Deleting appointment: {AppointmentId}", id);

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var appointment = await _db.Appointments
                .Where(a => branchIds.Contains(a.BranchId))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found: {AppointmentId}", id);
                return NotFound();
            }

            // Instead of hard delete, update status to Cancelled
            appointment.Status = AppointmentStatus.Cancelled;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Appointment cancelled successfully: {AppointmentId}", id);
            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment: {AppointmentId}", id);
            TempData["ErrorMessage"] = "An error occurred while cancelling the appointment.";
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
                DisplayName = p.FullNameEn + " (" + p.EmiratesId + ")"
            })
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "DisplayName");
    }

    // Helper method to load providers dropdown
    private async Task LoadProvidersDropdown()
    {
        var branchIds = await _branchService.GetUserBranchIdsAsync();
        var providers = await _db.Employees
            .Where(e => branchIds.Contains(e.BranchId) && e.IsActive)
            .OrderBy(e => e.FullNameEn)
            .Select(e => new
            {
                e.Id,
                DisplayName = e.FullNameEn + " - " + e.JobPosition.Name
            })
            .ToListAsync();

        ViewBag.Providers = new SelectList(providers, "Id", "DisplayName");
    }
}
