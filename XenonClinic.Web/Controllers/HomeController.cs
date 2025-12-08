using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<HomeController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Loading dashboard for user");

            var today = DateTime.UtcNow.Date;
            var branchIds = await _branchService.GetUserBranchIdsAsync();

            // Filter data by user's accessible branches
            var appointmentsQuery = branchIds.Any()
                ? _db.Appointments.Where(a => branchIds.Contains(a.BranchId))
                : _db.Appointments;

            var patientsQuery = branchIds.Any()
                ? _db.Patients.Where(p => branchIds.Contains(p.BranchId))
                : _db.Patients;

            var visitsQuery = branchIds.Any()
                ? _db.AudiologyVisits.Where(v => branchIds.Contains(v.BranchId))
                : _db.AudiologyVisits;

            var invoicesQuery = branchIds.Any()
                ? _db.Invoices.Where(i => branchIds.Contains(i.BranchId))
                : _db.Invoices;

            var viewModel = new DashboardViewModel
            {
                AppointmentsToday = await appointmentsQuery.CountAsync(a => a.StartTime.Date == today),
                ActivePatients = await patientsQuery.CountAsync(),
                VisitsThisWeek = await visitsQuery.CountAsync(v => v.VisitDate >= today.AddDays(-7)),
                RevenueThisMonth = await invoicesQuery
                    .Where(i => i.InvoiceDate.Month == today.Month && i.InvoiceDate.Year == today.Year)
                    .SumAsync(i => (decimal?)i.TotalAmount) ?? 0,
                UpcomingAppointments = await appointmentsQuery
                    .OrderBy(a => a.StartTime)
                    .Take(8)
                    .ToListAsync(),
                VisitsSeries = await visitsQuery
                    .Where(v => v.VisitDate >= today.AddDays(-7))
                    .GroupBy(v => v.VisitDate.Date)
                    .Select(g => new VisitSeriesData { Date = g.Key, Count = g.Count() })
                    .ToListAsync()
            };

            _logger.LogInformation(
                "Dashboard loaded successfully. Patients: {PatientCount}, Appointments: {AppointmentCount}",
                viewModel.ActivePatients,
                viewModel.AppointmentsToday);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            throw;
        }
    }
}
