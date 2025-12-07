using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly XenonClinicDbContext _db;

    public PatientsController(XenonClinicDbContext db)
    private readonly ClinicDbContext _db;

    public PatientsController(ClinicDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.FullNameEn.Contains(search) || p.EmiratesId.Contains(search));
        }

        var patients = await query.OrderByDescending(p => p.Id).Take(100).ToListAsync();
        ViewBag.Search = search;
        return View(patients);
    }

    public IActionResult Create()
    {
        return View(new Patient { DateOfBirth = DateTime.UtcNow.AddYears(-30) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (await _db.Patients.AnyAsync(p => p.EmiratesId == patient.EmiratesId))
        {
            ModelState.AddModelError(string.Empty, "A patient already exists with this Emirates ID.");
        }

        if (ModelState.IsValid)
        {
            if (patient.BranchId == 0)
            {
                patient.BranchId = await _db.Branches.Select(b => b.Id).FirstAsync();
            }

            _db.Add(patient);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(patient);
    }

    [HttpPost]
    public async Task<IActionResult> ReadEid()
    {
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
}
