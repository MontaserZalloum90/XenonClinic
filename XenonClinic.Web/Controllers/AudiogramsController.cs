using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class AudiogramsController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<AudiogramsController> _logger;

    public AudiogramsController(
        ClinicDbContext db,
        IBranchScopedService branchService,
        ILogger<AudiogramsController> logger)
    {
        _db = db;
        _branchService = branchService;
        _logger = logger;
    }

    // GET: Audiograms/Create?visitId=5
    public async Task<IActionResult> Create(int visitId)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var visit = await _db.AudiologyVisits
                .Include(v => v.Patient)
                .Include(v => v.Audiogram)
                .Where(v => branchIds.Contains(v.BranchId))
                .FirstOrDefaultAsync(v => v.Id == visitId);

            if (visit == null)
            {
                _logger.LogWarning("Audiology visit {VisitId} not found or access denied", visitId);
                return NotFound();
            }

            if (visit.Audiogram != null)
            {
                _logger.LogWarning("Audiogram already exists for visit {VisitId}", visitId);
                TempData["Error"] = "An audiogram already exists for this visit. Please edit the existing audiogram.";
                return RedirectToAction("Details", "AudiologyVisits", new { id = visitId });
            }

            var model = new CreateAudiogramViewModel
            {
                AudiologyVisitId = visitId
            };

            ViewBag.Visit = visit;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create audiogram form for visit: {VisitId}", visitId);
            throw;
        }
    }

    // POST: Audiograms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAudiogramViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var branchIds = await _branchService.GetUserBranchIdsAsync();
                var visit = await _db.AudiologyVisits
                    .Include(v => v.Audiogram)
                    .Where(v => branchIds.Contains(v.BranchId))
                    .FirstOrDefaultAsync(v => v.Id == model.AudiologyVisitId);

                if (visit == null)
                {
                    _logger.LogWarning("Audiology visit {VisitId} not found or access denied", model.AudiologyVisitId);
                    return NotFound();
                }

                if (visit.Audiogram != null)
                {
                    ModelState.AddModelError("", "An audiogram already exists for this visit.");
                    ViewBag.Visit = visit;
                    return View(model);
                }

                // Build audiogram data object
                var audiogramData = new
                {
                    leftEar = new
                    {
                        airConduction = new
                        {
                            hz125 = model.LeftEar_AC_125,
                            hz250 = model.LeftEar_AC_250,
                            hz500 = model.LeftEar_AC_500,
                            hz1000 = model.LeftEar_AC_1000,
                            hz2000 = model.LeftEar_AC_2000,
                            hz3000 = model.LeftEar_AC_3000,
                            hz4000 = model.LeftEar_AC_4000,
                            hz6000 = model.LeftEar_AC_6000,
                            hz8000 = model.LeftEar_AC_8000
                        },
                        boneConduction = new
                        {
                            hz250 = model.LeftEar_BC_250,
                            hz500 = model.LeftEar_BC_500,
                            hz1000 = model.LeftEar_BC_1000,
                            hz2000 = model.LeftEar_BC_2000,
                            hz3000 = model.LeftEar_BC_3000,
                            hz4000 = model.LeftEar_BC_4000
                        }
                    },
                    rightEar = new
                    {
                        airConduction = new
                        {
                            hz125 = model.RightEar_AC_125,
                            hz250 = model.RightEar_AC_250,
                            hz500 = model.RightEar_AC_500,
                            hz1000 = model.RightEar_AC_1000,
                            hz2000 = model.RightEar_AC_2000,
                            hz3000 = model.RightEar_AC_3000,
                            hz4000 = model.RightEar_AC_4000,
                            hz6000 = model.RightEar_AC_6000,
                            hz8000 = model.RightEar_AC_8000
                        },
                        boneConduction = new
                        {
                            hz250 = model.RightEar_BC_250,
                            hz500 = model.RightEar_BC_500,
                            hz1000 = model.RightEar_BC_1000,
                            hz2000 = model.RightEar_BC_2000,
                            hz3000 = model.RightEar_BC_3000,
                            hz4000 = model.RightEar_BC_4000
                        }
                    }
                };

                var audiogram = new Audiogram
                {
                    AudiologyVisitId = model.AudiologyVisitId,
                    RawDataJson = JsonSerializer.Serialize(audiogramData),
                    Notes = model.Notes
                };

                _db.Audiograms.Add(audiogram);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created audiogram {AudiogramId} for visit {VisitId}",
                    audiogram.Id, audiogram.AudiologyVisitId);

                TempData["Success"] = "Audiogram recorded successfully.";
                return RedirectToAction("Details", "AudiologyVisits", new { id = model.AudiologyVisitId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audiogram");
                ModelState.AddModelError("", "An error occurred while recording the audiogram.");
            }
        }

        var visitForView = await _db.AudiologyVisits
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.Id == model.AudiologyVisitId);
        ViewBag.Visit = visitForView;
        return View(model);
    }

    // GET: Audiograms/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var branchIds = await _branchService.GetUserBranchIdsAsync();
            var audiogram = await _db.Audiograms
                .Include(a => a.Visit)
                    .ThenInclude(v => v.Patient)
                .Where(a => branchIds.Contains(a.Visit.BranchId))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audiogram == null)
            {
                _logger.LogWarning("Audiogram {AudiogramId} not found or access denied", id);
                return NotFound();
            }

            var model = new EditAudiogramViewModel
            {
                Id = audiogram.Id,
                AudiologyVisitId = audiogram.AudiologyVisitId,
                Notes = audiogram.Notes
            };

            // Parse JSON data
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(audiogram.RawDataJson);

                // Left Ear - Air Conduction
                if (data.TryGetProperty("leftEar", out var leftEar) &&
                    leftEar.TryGetProperty("airConduction", out var leftAC))
                {
                    model.LeftEar_AC_125 = GetIntValue(leftAC, "hz125");
                    model.LeftEar_AC_250 = GetIntValue(leftAC, "hz250");
                    model.LeftEar_AC_500 = GetIntValue(leftAC, "hz500");
                    model.LeftEar_AC_1000 = GetIntValue(leftAC, "hz1000");
                    model.LeftEar_AC_2000 = GetIntValue(leftAC, "hz2000");
                    model.LeftEar_AC_3000 = GetIntValue(leftAC, "hz3000");
                    model.LeftEar_AC_4000 = GetIntValue(leftAC, "hz4000");
                    model.LeftEar_AC_6000 = GetIntValue(leftAC, "hz6000");
                    model.LeftEar_AC_8000 = GetIntValue(leftAC, "hz8000");
                }

                // Left Ear - Bone Conduction
                if (leftEar.TryGetProperty("boneConduction", out var leftBC))
                {
                    model.LeftEar_BC_250 = GetIntValue(leftBC, "hz250");
                    model.LeftEar_BC_500 = GetIntValue(leftBC, "hz500");
                    model.LeftEar_BC_1000 = GetIntValue(leftBC, "hz1000");
                    model.LeftEar_BC_2000 = GetIntValue(leftBC, "hz2000");
                    model.LeftEar_BC_3000 = GetIntValue(leftBC, "hz3000");
                    model.LeftEar_BC_4000 = GetIntValue(leftBC, "hz4000");
                }

                // Right Ear - Air Conduction
                if (data.TryGetProperty("rightEar", out var rightEar) &&
                    rightEar.TryGetProperty("airConduction", out var rightAC))
                {
                    model.RightEar_AC_125 = GetIntValue(rightAC, "hz125");
                    model.RightEar_AC_250 = GetIntValue(rightAC, "hz250");
                    model.RightEar_AC_500 = GetIntValue(rightAC, "hz500");
                    model.RightEar_AC_1000 = GetIntValue(rightAC, "hz1000");
                    model.RightEar_AC_2000 = GetIntValue(rightAC, "hz2000");
                    model.RightEar_AC_3000 = GetIntValue(rightAC, "hz3000");
                    model.RightEar_AC_4000 = GetIntValue(rightAC, "hz4000");
                    model.RightEar_AC_6000 = GetIntValue(rightAC, "hz6000");
                    model.RightEar_AC_8000 = GetIntValue(rightAC, "hz8000");
                }

                // Right Ear - Bone Conduction
                if (rightEar.TryGetProperty("boneConduction", out var rightBC))
                {
                    model.RightEar_BC_250 = GetIntValue(rightBC, "hz250");
                    model.RightEar_BC_500 = GetIntValue(rightBC, "hz500");
                    model.RightEar_BC_1000 = GetIntValue(rightBC, "hz1000");
                    model.RightEar_BC_2000 = GetIntValue(rightBC, "hz2000");
                    model.RightEar_BC_3000 = GetIntValue(rightBC, "hz3000");
                    model.RightEar_BC_4000 = GetIntValue(rightBC, "hz4000");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing audiogram data for audiogram: {AudiogramId}", id);
            }

            ViewBag.Visit = audiogram.Visit;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for audiogram: {AudiogramId}", id);
            throw;
        }
    }

    // POST: Audiograms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditAudiogramViewModel model)
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
                var audiogram = await _db.Audiograms
                    .Include(a => a.Visit)
                    .Where(a => branchIds.Contains(a.Visit.BranchId))
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (audiogram == null)
                {
                    _logger.LogWarning("Audiogram {AudiogramId} not found or access denied", id);
                    return NotFound();
                }

                // Build updated audiogram data object
                var audiogramData = new
                {
                    leftEar = new
                    {
                        airConduction = new
                        {
                            hz125 = model.LeftEar_AC_125,
                            hz250 = model.LeftEar_AC_250,
                            hz500 = model.LeftEar_AC_500,
                            hz1000 = model.LeftEar_AC_1000,
                            hz2000 = model.LeftEar_AC_2000,
                            hz3000 = model.LeftEar_AC_3000,
                            hz4000 = model.LeftEar_AC_4000,
                            hz6000 = model.LeftEar_AC_6000,
                            hz8000 = model.LeftEar_AC_8000
                        },
                        boneConduction = new
                        {
                            hz250 = model.LeftEar_BC_250,
                            hz500 = model.LeftEar_BC_500,
                            hz1000 = model.LeftEar_BC_1000,
                            hz2000 = model.LeftEar_BC_2000,
                            hz3000 = model.LeftEar_BC_3000,
                            hz4000 = model.LeftEar_BC_4000
                        }
                    },
                    rightEar = new
                    {
                        airConduction = new
                        {
                            hz125 = model.RightEar_AC_125,
                            hz250 = model.RightEar_AC_250,
                            hz500 = model.RightEar_AC_500,
                            hz1000 = model.RightEar_AC_1000,
                            hz2000 = model.RightEar_AC_2000,
                            hz3000 = model.RightEar_AC_3000,
                            hz4000 = model.RightEar_AC_4000,
                            hz6000 = model.RightEar_AC_6000,
                            hz8000 = model.RightEar_AC_8000
                        },
                        boneConduction = new
                        {
                            hz250 = model.RightEar_BC_250,
                            hz500 = model.RightEar_BC_500,
                            hz1000 = model.RightEar_BC_1000,
                            hz2000 = model.RightEar_BC_2000,
                            hz3000 = model.RightEar_BC_3000,
                            hz4000 = model.RightEar_BC_4000
                        }
                    }
                };

                audiogram.RawDataJson = JsonSerializer.Serialize(audiogramData);
                audiogram.Notes = model.Notes;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Updated audiogram {AudiogramId}", id);
                TempData["Success"] = "Audiogram updated successfully.";

                return RedirectToAction("Details", "AudiologyVisits", new { id = audiogram.AudiologyVisitId });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating audiogram: {AudiogramId}", id);
                ModelState.AddModelError("", "The audiogram was modified by another user. Please refresh and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating audiogram: {AudiogramId}", id);
                ModelState.AddModelError("", "An error occurred while updating the audiogram.");
            }
        }

        var visitForView = await _db.AudiologyVisits
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.Id == model.AudiologyVisitId);
        ViewBag.Visit = visitForView;
        return View(model);
    }

    // Helper method to get int value from JSON element
    private int? GetIntValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt32();
            }
            else if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
        }
        return null;
    }
}
