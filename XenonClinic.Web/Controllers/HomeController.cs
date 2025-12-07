using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly XenonClinicDbContext _db;

    public HomeController(XenonClinicDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var appointmentsToday = await _db.Appointments.CountAsync(a => a.StartTime.Date == today);
        var activePatients = await _db.Patients.CountAsync();
        var visitsThisWeek = await _db.AudiologyVisits.CountAsync(v => v.VisitDate >= today.AddDays(-7));
        var revenueThisMonth = await _db.Invoices.Where(i => i.InvoiceDate.Month == today.Month && i.InvoiceDate.Year == today.Year).SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
        var upcoming = await _db.Appointments.OrderBy(a => a.StartTime).Take(8).ToListAsync();
        var visitsSeries = await _db.AudiologyVisits
            .Where(v => v.VisitDate >= today.AddDays(-7))
            .GroupBy(v => v.VisitDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.VisitsSeries = visitsSeries;
        ViewBag.AppointmentsToday = appointmentsToday;
        ViewBag.ActivePatients = activePatients;
        ViewBag.VisitsThisWeek = visitsThisWeek;
        ViewBag.RevenueThisMonth = revenueThisMonth;
        return View(upcoming);
    }
}
