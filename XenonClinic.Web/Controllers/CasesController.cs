using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class CasesController : Controller
{
    private readonly ICaseService _caseService;
    private readonly IBranchScopedService _branchService;
    private readonly ITenantService _tenantService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _context;

    public CasesController(
        ICaseService caseService,
        IBranchScopedService branchService,
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        ClinicDbContext context)
    {
        _caseService = caseService;
        _branchService = branchService;
        _tenantService = tenantService;
        _userManager = userManager;
        _context = context;
    }

    // ==================== Case List ====================

    public async Task<IActionResult> Index(int? branchId, int? statusId, string? searchTerm, bool includeClosed = false)
    {
        var currentBranchId = branchId ?? await _branchService.GetCurrentBranchIdAsync();
        if (currentBranchId == 0)
        {
            return RedirectToAction("SelectBranch", "Home");
        }

        var branch = await _context.Branches.FindAsync(currentBranchId);
        var tenantId = await _tenantService.GetCurrentTenantIdAsync();

        List<Case> cases;
        if (!string.IsNullOrEmpty(searchTerm))
        {
            cases = await _caseService.SearchCasesAsync(searchTerm, currentBranchId);
        }
        else if (statusId.HasValue)
        {
            cases = await _caseService.GetCasesByStatusAsync(statusId.Value);
            cases = cases.Where(c => c.BranchId == currentBranchId).ToList();
        }
        else
        {
            cases = await _caseService.GetCasesByBranchIdAsync(currentBranchId, includeClosed);
        }

        var model = new CaseListViewModel
        {
            BranchId = currentBranchId,
            BranchName = branch?.Name,
            StatusId = statusId,
            SearchTerm = searchTerm,
            IncludeClosed = includeClosed,
            Cases = cases.Select(c => new CaseListItemViewModel
            {
                Id = c.Id,
                CaseNumber = c.CaseNumber,
                Title = c.Title,
                PatientName = $"{c.Patient.FullNameEn}",
                BranchName = c.Branch.Name,
                CaseTypeName = c.CaseType.Name,
                CaseStatusName = c.CaseStatus.Name,
                AssignedToName = c.AssignedToUser?.DisplayName,
                Priority = c.Priority,
                OpenedDate = c.OpenedDate,
                ClosedDate = c.ClosedDate,
                TargetDate = c.TargetDate,
                StatusColorCode = c.CaseStatus.ColorCode,
                PendingActivitiesCount = c.Activities.Count(a => a.Status == CaseActivityStatus.Pending),
                NotesCount = c.Notes.Count
            }).ToList(),
            Statistics = await _caseService.GetCaseStatisticsByBranchAsync(currentBranchId)
        };

        if (tenantId.HasValue)
        {
            model.CaseTypes = await _context.CaseTypes
                .Where(ct => ct.TenantId == tenantId.Value && ct.IsActive)
                .OrderBy(ct => ct.DisplayOrder)
                .Select(ct => new CaseTypeSelectItem
                {
                    Id = ct.Id,
                    Name = ct.Name,
                    ColorCode = ct.ColorCode,
                    IconClass = ct.IconClass
                }).ToListAsync();

            model.CaseStatuses = await _context.CaseStatuses
                .Where(cs => cs.TenantId == tenantId.Value && cs.IsActive)
                .OrderBy(cs => cs.DisplayOrder)
                .Select(cs => new CaseStatusSelectItem
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    ColorCode = cs.ColorCode,
                    Category = cs.Category
                }).ToListAsync();
        }

        return View(model);
    }

    // ==================== Case Details ====================

    public async Task<IActionResult> Details(int id)
    {
        var caseEntity = await _caseService.GetCaseByIdAsync(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var notes = await _caseService.GetCaseNotesAsync(id);
        var activities = await _caseService.GetCaseActivitiesAsync(id);
        var pendingActivities = await _caseService.GetPendingActivitiesAsync(id);
        var overdueActivities = await _caseService.GetOverdueActivitiesAsync(id);

        var model = new CaseDetailsViewModel
        {
            Case = caseEntity,
            Notes = notes,
            Activities = activities,
            PendingActivities = pendingActivities,
            OverdueActivities = overdueActivities,
            NewNote = new CaseNoteFormViewModel { CaseId = id },
            NewActivity = new CaseActivityFormViewModel { CaseId = id }
        };

        return View(model);
    }

    // ==================== Create Case ====================

    public async Task<IActionResult> Create(int? patientId)
    {
        var currentBranchId = await _branchService.GetCurrentBranchIdAsync();
        if (currentBranchId == 0)
        {
            return RedirectToAction("SelectBranch", "Home");
        }

        var tenantId = await _tenantService.GetCurrentTenantIdAsync();
        var model = new CaseFormViewModel
        {
            BranchId = currentBranchId,
            PatientId = patientId ?? 0
        };

        if (patientId.HasValue)
        {
            var patient = await _context.Patients.FindAsync(patientId.Value);
            if (patient != null)
            {
                model.PatientName = patient.FullNameEn;
            }
        }

        await PopulateFormSelectListsAsync(model, tenantId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CaseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var tenantId = await _tenantService.GetCurrentTenantIdAsync();
            await PopulateFormSelectListsAsync(model, tenantId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var caseEntity = new Case
        {
            PatientId = model.PatientId,
            BranchId = model.BranchId,
            CaseTypeId = model.CaseTypeId,
            CaseStatusId = model.CaseStatusId,
            AssignedToUserId = model.AssignedToUserId,
            Title = model.Title,
            Description = model.Description,
            Priority = model.Priority,
            ChiefComplaint = model.ChiefComplaint,
            TargetDate = model.TargetDate,
            Tags = model.Tags,
            OpenedBy = user?.Id,
            CreatedBy = user?.Id
        };

        await _caseService.CreateCaseAsync(caseEntity);
        TempData["Success"] = $"Case {caseEntity.CaseNumber} created successfully.";

        return RedirectToAction(nameof(Details), new { id = caseEntity.Id });
    }

    // ==================== Edit Case ====================

    public async Task<IActionResult> Edit(int id)
    {
        var caseEntity = await _caseService.GetCaseByIdAsync(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var tenantId = await _tenantService.GetCurrentTenantIdAsync();
        var model = new CaseFormViewModel
        {
            Id = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            PatientId = caseEntity.PatientId,
            BranchId = caseEntity.BranchId,
            CaseTypeId = caseEntity.CaseTypeId,
            CaseStatusId = caseEntity.CaseStatusId,
            AssignedToUserId = caseEntity.AssignedToUserId,
            Title = caseEntity.Title,
            Description = caseEntity.Description,
            Priority = caseEntity.Priority,
            ChiefComplaint = caseEntity.ChiefComplaint,
            TargetDate = caseEntity.TargetDate,
            Tags = caseEntity.Tags,
            PatientName = caseEntity.Patient.FullNameEn,
            BranchName = caseEntity.Branch.Name
        };

        await PopulateFormSelectListsAsync(model, tenantId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CaseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var tenantId = await _tenantService.GetCurrentTenantIdAsync();
            await PopulateFormSelectListsAsync(model, tenantId);
            return View(model);
        }

        var caseEntity = await _context.Cases.FindAsync(model.Id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        caseEntity.CaseTypeId = model.CaseTypeId;
        caseEntity.CaseStatusId = model.CaseStatusId;
        caseEntity.AssignedToUserId = model.AssignedToUserId;
        caseEntity.Title = model.Title;
        caseEntity.Description = model.Description;
        caseEntity.Priority = model.Priority;
        caseEntity.ChiefComplaint = model.ChiefComplaint;
        caseEntity.TargetDate = model.TargetDate;
        caseEntity.Tags = model.Tags;
        caseEntity.UpdatedBy = user?.Id;

        await _caseService.UpdateCaseAsync(caseEntity);
        TempData["Success"] = "Case updated successfully.";

        return RedirectToAction(nameof(Details), new { id = caseEntity.Id });
    }

    // ==================== Case Status Management ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int caseId, int newStatusId, string? notes)
    {
        var success = await _caseService.ChangeCaseStatusAsync(caseId, newStatusId, notes);
        if (success)
        {
            TempData["Success"] = "Case status updated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to update case status.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    public async Task<IActionResult> Close(int id)
    {
        var caseEntity = await _caseService.GetCaseByIdAsync(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var model = new CloseCaseViewModel
        {
            CaseId = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            Title = caseEntity.Title
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(CloseCaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var success = await _caseService.CloseCaseAsync(model.CaseId, model.Resolution, user?.Id ?? "System");

        if (success)
        {
            TempData["Success"] = "Case closed successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to close case.";
        }

        return RedirectToAction(nameof(Details), new { id = model.CaseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(int id)
    {
        var success = await _caseService.ReopenCaseAsync(id);
        if (success)
        {
            TempData["Success"] = "Case reopened successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to reopen case.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // ==================== Case Assignment ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int caseId, string userId)
    {
        var success = await _caseService.AssignCaseAsync(caseId, userId);
        if (success)
        {
            TempData["Success"] = "Case assigned successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to assign case.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(int caseId)
    {
        var success = await _caseService.UnassignCaseAsync(caseId);
        if (success)
        {
            TempData["Success"] = "Case unassigned successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to unassign case.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    // ==================== Case Notes ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(CaseNoteFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Failed to add note. Please check your input.";
            return RedirectToAction(nameof(Details), new { id = model.CaseId });
        }

        var user = await _userManager.GetUserAsync(User);
        var note = new CaseNote
        {
            CaseId = model.CaseId,
            Content = model.Content,
            NoteType = model.NoteType,
            IsVisibleToPatient = model.IsVisibleToPatient,
            IsPinned = model.IsPinned,
            CreatedBy = user?.Id
        };

        await _caseService.AddCaseNoteAsync(note);
        TempData["Success"] = "Note added successfully.";

        return RedirectToAction(nameof(Details), new { id = model.CaseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(int id, int caseId)
    {
        var success = await _caseService.DeleteCaseNoteAsync(id);
        if (success)
        {
            TempData["Success"] = "Note deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete note.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    // ==================== Case Activities ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddActivity(CaseActivityFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Failed to add activity. Please check your input.";
            return RedirectToAction(nameof(Details), new { id = model.CaseId });
        }

        var user = await _userManager.GetUserAsync(User);
        var activity = new CaseActivity
        {
            CaseId = model.CaseId,
            Title = model.Title,
            Description = model.Description,
            ActivityType = model.ActivityType,
            Status = model.Status,
            AssignedToUserId = model.AssignedToUserId,
            DueDate = model.DueDate,
            Priority = model.Priority,
            CreatedBy = user?.Id
        };

        await _caseService.AddCaseActivityAsync(activity);
        TempData["Success"] = "Activity added successfully.";

        return RedirectToAction(nameof(Details), new { id = model.CaseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteActivity(int id, int caseId, string? result)
    {
        var success = await _caseService.CompleteCaseActivityAsync(id, result);
        if (success)
        {
            TempData["Success"] = "Activity marked as completed.";
        }
        else
        {
            TempData["Error"] = "Failed to complete activity.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActivity(int id, int caseId)
    {
        var success = await _caseService.DeleteCaseActivityAsync(id);
        if (success)
        {
            TempData["Success"] = "Activity deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete activity.";
        }

        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    // ==================== My Cases ====================

    public async Task<IActionResult> MyCases()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cases = await _caseService.GetCasesByAssignedUserAsync(user.Id);

        var model = new CaseListViewModel
        {
            AssignedToUserId = user.Id,
            Cases = cases.Select(c => new CaseListItemViewModel
            {
                Id = c.Id,
                CaseNumber = c.CaseNumber,
                Title = c.Title,
                PatientName = $"{c.Patient.FullNameEn}",
                BranchName = c.Branch.Name,
                CaseTypeName = c.CaseType.Name,
                CaseStatusName = c.CaseStatus.Name,
                Priority = c.Priority,
                OpenedDate = c.OpenedDate,
                TargetDate = c.TargetDate,
                StatusColorCode = c.CaseStatus.ColorCode,
                PendingActivitiesCount = c.Activities.Count(a => a.Status == CaseActivityStatus.Pending),
                NotesCount = c.Notes.Count
            }).ToList()
        };

        return View("Index", model);
    }

    // ==================== Helper Methods ====================

    private async Task PopulateFormSelectListsAsync(CaseFormViewModel model, int? tenantId)
    {
        if (!tenantId.HasValue)
            return;

        // Case types
        ViewBag.CaseTypes = await _context.CaseTypes
            .Where(ct => ct.TenantId == tenantId.Value && ct.IsActive)
            .OrderBy(ct => ct.DisplayOrder)
            .ToListAsync();

        // Case statuses
        ViewBag.CaseStatuses = await _context.CaseStatuses
            .Where(cs => cs.TenantId == tenantId.Value && cs.IsActive)
            .OrderBy(cs => cs.DisplayOrder)
            .ToListAsync();

        // Patients from current branch
        ViewBag.Patients = await _context.Patients
            .Where(p => p.BranchId == model.BranchId)
            .OrderBy(p => p.FullNameEn)
            .Select(p => new PatientSelectItem
            {
                Id = p.Id,
                Name = p.FullNameEn,
                EmiratesId = p.EmiratesId
            })
            .ToListAsync();

        // Users (providers) from current branch
        ViewBag.Users = await _context.Users
            .Where(u => u.PrimaryBranchId == model.BranchId && u.IsActive)
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserSelectItem
            {
                Id = u.Id,
                Name = u.DisplayName ?? u.Email!,
                Email = u.Email!
            })
            .ToListAsync();
    }

    // ==================== Delete Case ====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,TenantAdmin,CompanyAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _caseService.DeleteCaseAsync(id);
        if (success)
        {
            TempData["Success"] = "Case deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to delete case.";
        }

        return RedirectToAction(nameof(Index));
    }
}
