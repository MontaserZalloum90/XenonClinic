using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Controllers;

[Authorize(Roles = "Admin,BranchAdmin,Audiologist")]
public class AnalyticsController : Controller
{
    private readonly XenonClinicDbContext _db;

    public AnalyticsController(XenonClinicDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var totalPatients = await _db.Patients.CountAsync();
        var totalAppointments = await _db.Appointments.CountAsync();
        var completedVisits = await _db.AudiologyVisits.CountAsync();
        var totalRevenue = await _db.Invoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        var appointmentSeries = await _db.Appointments
            .Where(a => a.StartTime >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(a => a.StartTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var revenueSeries = await _db.Invoices
            .Where(i => i.InvoiceDate >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(i => i.InvoiceDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .ToListAsync();

        var appointmentTypes = await _db.Appointments
            .GroupBy(a => a.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        var deviceModels = await _db.HearingDevices
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
        return View();
    }

    public async Task<IActionResult> Audiology()
    {
        var diagnoses = await _db.AudiologyVisits
            .GroupBy(v => v.Diagnosis ?? "Unspecified")
            .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        var hearingLoss = await _db.Patients
            .GroupBy(p => p.HearingLossType ?? "Normal")
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        var ageBuckets = new Dictionary<string, int> { { "<20", 0 }, { "20-40", 0 }, { "40-60", 0 }, { "60+", 0 } };
        var now = DateTime.UtcNow;
        var patients = await _db.Patients.Select(p => p.DateOfBirth).ToListAsync();
        foreach (var dob in patients)
        {
            var age = (int)((now - dob).TotalDays / 365.25);
            if (age < 20) ageBuckets["<20"]++;
            else if (age < 40) ageBuckets["20-40"]++;
            else if (age < 60) ageBuckets["40-60"]++;
            else ageBuckets["60+"]++;
        }

        var repeatVisitRate = await _db.Patients
            .Select(p => new { p.Id, VisitCount = _db.AudiologyVisits.Count(v => v.PatientId == p.Id) })
            .Where(x => x.VisitCount > 3)
            .CountAsync();

        ViewBag.Diagnoses = diagnoses;
        ViewBag.HearingLoss = hearingLoss;
        ViewBag.AgeBuckets = ageBuckets;
        ViewBag.RepeatVisitRate = repeatVisitRate;
        return View();
    }
}
