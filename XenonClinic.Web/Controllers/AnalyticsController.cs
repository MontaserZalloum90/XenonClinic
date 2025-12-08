using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Controllers;

[Authorize(Roles = "Admin,BranchAdmin,Audiologist")]
public class AnalyticsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<AnalyticsController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Loading analytics dashboard");

            var branchIds = await _branchService.GetUserBranchIdsAsync();

            // Apply branch filtering
            var patientsQuery = branchIds.Any() ? _db.Patients.Where(p => branchIds.Contains(p.BranchId)) : _db.Patients;
            var appointmentsQuery = branchIds.Any() ? _db.Appointments.Where(a => branchIds.Contains(a.BranchId)) : _db.Appointments;
            var visitsQuery = branchIds.Any() ? _db.AudiologyVisits.Where(v => branchIds.Contains(v.BranchId)) : _db.AudiologyVisits;
            var invoicesQuery = branchIds.Any() ? _db.Invoices.Where(i => branchIds.Contains(i.BranchId)) : _db.Invoices;
            var devicesQuery = branchIds.Any() ? _db.HearingDevices.Where(d => branchIds.Contains(d.BranchId)) : _db.HearingDevices;

            var totalPatients = await patientsQuery.CountAsync();
            var totalAppointments = await appointmentsQuery.CountAsync();
            var completedVisits = await visitsQuery.CountAsync();
            var totalRevenue = await invoicesQuery.SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            var appointmentSeries = await appointmentsQuery
                .Where(a => a.StartTime >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var revenueSeries = await invoicesQuery
                .Where(i => i.InvoiceDate >= DateTime.UtcNow.AddDays(-30))
                .GroupBy(i => i.InvoiceDate.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            var appointmentTypes = await appointmentsQuery
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var deviceModels = await devicesQuery
                .GroupBy(d => d.ModelName)
                .Select(g => new { Model = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.AppointmentSeries = appointmentSeries;
            ViewBag.RevenueSeries = revenueSeries;
            ViewBag.AppointmentTypes = appointmentTypes;
            ViewBag.DeviceModels = deviceModels;

            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.CompletedVisits = completedVisits;
            ViewBag.TotalRevenue = totalRevenue;

            _logger.LogInformation("Analytics dashboard loaded successfully");
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading analytics dashboard");
            throw;
        }
    }

    public async Task<IActionResult> Audiology()
    {
        try
        {
            _logger.LogInformation("Loading audiology analytics");

            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var visitsQuery = branchIds.Any() ? _db.AudiologyVisits.Where(v => branchIds.Contains(v.BranchId)) : _db.AudiologyVisits;
            var patientsQuery = branchIds.Any() ? _db.Patients.Where(p => branchIds.Contains(p.BranchId)) : _db.Patients;

            var diagnoses = await visitsQuery
                .GroupBy(v => v.Diagnosis ?? "Unspecified")
                .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();

            var hearingLoss = await patientsQuery
                .GroupBy(p => p.HearingLossType ?? "Normal")
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var ageBuckets = new Dictionary<string, int> { { "<20", 0 }, { "20-40", 0 }, { "40-60", 0 }, { "60+", 0 } };
            var now = DateTime.UtcNow;
            var patients = await patientsQuery.Select(p => p.DateOfBirth).ToListAsync();
        foreach (var dob in patients)
        {
            var age = (int)((now - dob).TotalDays / 365.25);
            if (age < 20) ageBuckets["<20"]++;
            else if (age < 40) ageBuckets["20-40"]++;
            else if (age < 60) ageBuckets["40-60"]++;
            else ageBuckets["60+"]++;
        }

            var repeatVisitRate = await patientsQuery
                .Select(p => new { p.Id, VisitCount = _db.AudiologyVisits.Count(v => v.PatientId == p.Id) })
                .Where(x => x.VisitCount > 3)
                .CountAsync();

            ViewBag.Diagnoses = diagnoses;
            ViewBag.HearingLoss = hearingLoss;
            ViewBag.AgeBuckets = ageBuckets;
            ViewBag.RepeatVisitRate = repeatVisitRate;

            _logger.LogInformation("Audiology analytics loaded successfully");
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audiology analytics");
            throw;
        }
    }
}
