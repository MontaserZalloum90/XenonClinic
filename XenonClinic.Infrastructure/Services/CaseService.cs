using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

public class CaseService : ICaseService
{
    private readonly ClinicDbContext _context;

    public CaseService(ClinicDbContext context)
    {
        _context = context;
    }

    // ==================== Case CRUD Operations ====================

    public async Task<Case> CreateCaseAsync(Case caseEntity)
    {
        if (string.IsNullOrEmpty(caseEntity.CaseNumber))
        {
            caseEntity.CaseNumber = await GenerateCaseNumberAsync(caseEntity.BranchId);
        }

        caseEntity.OpenedDate = DateTime.UtcNow;
        caseEntity.CreatedAt = DateTime.UtcNow;

        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync();

        return caseEntity;
    }

    public async Task<Case> UpdateCaseAsync(Case caseEntity)
    {
        caseEntity.UpdatedAt = DateTime.UtcNow;
        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();
        return caseEntity;
    }

    public async Task<Case?> GetCaseByIdAsync(int caseId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Notes)
            .Include(c => c.Activities)
            .FirstOrDefaultAsync(c => c.Id == caseId);
    }

    public async Task<Case?> GetCaseByNumberAsync(string caseNumber)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);
    }

    public async Task<List<Case>> GetCasesByPatientIdAsync(int patientId)
    {
        return await _context.Cases
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<List<Case>> GetCasesByBranchIdAsync(int branchId, bool includeInactive = false)
    {
        var query = _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .Where(c => c.BranchId == branchId);

        if (!includeInactive)
        {
            query = query.Where(c => c.ClosedDate == null);
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<List<Case>> GetCasesByAssignedUserAsync(string userId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.CasePriority)
            .Where(c => c.AssignedToUserId == userId && c.ClosedDate == null)
            .OrderByDescending(c => c.CasePriority.DisplayOrder)
            .ThenBy(c => c.TargetDate)
            .ToListAsync();
    }

    public async Task<List<Case>> GetCasesByStatusAsync(int statusId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.AssignedToUser)
            .Where(c => c.CaseStatusId == statusId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteCaseAsync(int caseId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        _context.Cases.Remove(caseEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Case Status Management ====================

    public async Task<bool> ChangeCaseStatusAsync(int caseId, int newStatusId, string? notes = null)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        var status = await _context.CaseStatuses.FindAsync(newStatusId);
        if (status == null)
            return false;

        caseEntity.CaseStatusId = newStatusId;
        caseEntity.UpdatedAt = DateTime.UtcNow;

        if (status.IsClosedStatus && caseEntity.ClosedDate == null)
        {
            caseEntity.ClosedDate = DateTime.UtcNow;
        }

        if (notes != null)
        {
            // Get Administrative note type lookup
            var adminNoteType = await _context.CaseNoteTypeLookups
                .FirstOrDefaultAsync(t => t.Code == "ADMINISTRATIVE" && t.IsActive);

            if (adminNoteType != null)
            {
                var note = new CaseNote
                {
                    CaseId = caseId,
                    Content = notes,
                    CaseNoteTypeId = adminNoteType.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CaseNotes.Add(note);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseCaseAsync(int caseId, string resolution, string closedBy)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        caseEntity.ClosedDate = DateTime.UtcNow;
        caseEntity.ClosedBy = closedBy;
        caseEntity.Resolution = resolution;
        caseEntity.UpdatedAt = DateTime.UtcNow;

        // Find a closed status
        var closedStatus = await _context.CaseStatuses
            .Where(s => s.IsClosedStatus)
            .FirstOrDefaultAsync();

        if (closedStatus != null)
        {
            caseEntity.CaseStatusId = closedStatus.Id;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReopenCaseAsync(int caseId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        caseEntity.ClosedDate = null;
        caseEntity.ClosedBy = null;
        caseEntity.UpdatedAt = DateTime.UtcNow;

        // Find an open status
        var openStatus = await _context.CaseStatuses
            .Where(s => s.Category == "Open" || s.Category == "InProgress")
            .FirstOrDefaultAsync();

        if (openStatus != null)
        {
            caseEntity.CaseStatusId = openStatus.Id;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Case Assignment ====================

    public async Task<bool> AssignCaseAsync(int caseId, string userId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        caseEntity.AssignedToUserId = userId;
        caseEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnassignCaseAsync(int caseId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            return false;

        caseEntity.AssignedToUserId = null;
        caseEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Case Notes ====================

    public async Task<CaseNote> AddCaseNoteAsync(CaseNote note)
    {
        note.CreatedAt = DateTime.UtcNow;
        _context.CaseNotes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<CaseNote> UpdateCaseNoteAsync(CaseNote note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        _context.CaseNotes.Update(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<List<CaseNote>> GetCaseNotesAsync(int caseId)
    {
        return await _context.CaseNotes
            .Where(n => n.CaseId == caseId)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteCaseNoteAsync(int noteId)
    {
        var note = await _context.CaseNotes.FindAsync(noteId);
        if (note == null)
            return false;

        _context.CaseNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Case Activities ====================

    public async Task<CaseActivity> AddCaseActivityAsync(CaseActivity activity)
    {
        activity.CreatedAt = DateTime.UtcNow;
        _context.CaseActivities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<CaseActivity> UpdateCaseActivityAsync(CaseActivity activity)
    {
        activity.UpdatedAt = DateTime.UtcNow;
        _context.CaseActivities.Update(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task<List<CaseActivity>> GetCaseActivitiesAsync(int caseId)
    {
        return await _context.CaseActivities
            .Include(a => a.AssignedToUser)
            .Include(a => a.CaseActivityStatus)
            .Where(a => a.CaseId == caseId)
            .OrderBy(a => a.CaseActivityStatus.DisplayOrder)
            .ThenBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<List<CaseActivity>> GetPendingActivitiesAsync(int caseId)
    {
        // Get Pending status lookup
        var pendingStatus = await _context.CaseActivityStatusLookups
            .FirstOrDefaultAsync(s => s.Code == "PENDING" && s.IsActive);

        if (pendingStatus == null)
            return new List<CaseActivity>();

        return await _context.CaseActivities
            .Include(a => a.AssignedToUser)
            .Where(a => a.CaseId == caseId && a.CaseActivityStatusId == pendingStatus.Id)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<List<CaseActivity>> GetOverdueActivitiesAsync(int caseId)
    {
        // Get Completed and Cancelled status lookups
        var completedCancelledStatuses = await _context.CaseActivityStatusLookups
            .Where(s => (s.Code == "COMPLETED" || s.Code == "CANCELLED") && s.IsActive)
            .Select(s => s.Id)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        return await _context.CaseActivities
            .Include(a => a.AssignedToUser)
            .Where(a => a.CaseId == caseId &&
                       !completedCancelledStatuses.Contains(a.CaseActivityStatusId) &&
                       a.DueDate.HasValue &&
                       a.DueDate.Value.Date < today)
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<bool> CompleteCaseActivityAsync(int activityId, string? result = null)
    {
        var activity = await _context.CaseActivities.FindAsync(activityId);
        if (activity == null)
            return false;

        // Get Completed status lookup
        var completedStatus = await _context.CaseActivityStatusLookups
            .FirstOrDefaultAsync(s => s.Code == "COMPLETED" && s.IsActive);

        if (completedStatus == null)
            return false;

        activity.CaseActivityStatusId = completedStatus.Id;
        activity.CompletedDate = DateTime.UtcNow;
        activity.Result = result;
        activity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCaseActivityAsync(int activityId)
    {
        var activity = await _context.CaseActivities.FindAsync(activityId);
        if (activity == null)
            return false;

        _context.CaseActivities.Remove(activity);
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Case Types and Statuses ====================

    public async Task<List<CaseType>> GetCaseTypesAsync(int tenantId)
    {
        return await _context.CaseTypes
            .Where(ct => ct.TenantId == tenantId && ct.IsActive)
            .OrderBy(ct => ct.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<CaseStatus>> GetCaseStatusesAsync(int tenantId)
    {
        return await _context.CaseStatuses
            .Where(cs => cs.TenantId == tenantId && cs.IsActive)
            .OrderBy(cs => cs.DisplayOrder)
            .ToListAsync();
    }

    public async Task<CaseType> CreateCaseTypeAsync(CaseType caseType)
    {
        caseType.CreatedAt = DateTime.UtcNow;
        _context.CaseTypes.Add(caseType);
        await _context.SaveChangesAsync();
        return caseType;
    }

    public async Task<CaseStatus> CreateCaseStatusAsync(CaseStatus caseStatus)
    {
        caseStatus.CreatedAt = DateTime.UtcNow;
        _context.CaseStatuses.Add(caseStatus);
        await _context.SaveChangesAsync();
        return caseStatus;
    }

    // ==================== Case Number Generation ====================

    public async Task<string> GenerateCaseNumberAsync(int branchId)
    {
        var branch = await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        var year = DateTime.UtcNow.Year;
        var branchCode = branch?.Code ?? "BR";

        // Get the count of cases for this branch this year
        var startOfYear = new DateTime(year, 1, 1);
        var count = await _context.Cases
            .Where(c => c.BranchId == branchId && c.CreatedAt >= startOfYear)
            .CountAsync();

        var sequence = (count + 1).ToString("D4");
        return $"CASE-{branchCode}-{year}-{sequence}";
    }

    // ==================== Dashboard and Statistics ====================

    public async Task<Dictionary<string, int>> GetCaseStatisticsByBranchAsync(int branchId)
    {
        var stats = new Dictionary<string, int>();

        // Get High and Urgent priority IDs
        var highPriorityIds = await _context.CasePriorityLookups
            .Where(p => (p.Code == "HIGH" || p.Code == "URGENT") && p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        var cases = await _context.Cases
            .Where(c => c.BranchId == branchId)
            .ToListAsync();

        stats["Total"] = cases.Count;
        stats["Open"] = cases.Count(c => c.ClosedDate == null);
        stats["Closed"] = cases.Count(c => c.ClosedDate != null);
        stats["High Priority"] = cases.Count(c => highPriorityIds.Contains(c.CasePriorityId));
        stats["Overdue"] = cases.Count(c => c.TargetDate.HasValue && c.TargetDate.Value < DateTime.UtcNow && c.ClosedDate == null);

        return stats;
    }

    public async Task<List<Case>> GetRecentCasesAsync(int branchId, int count = 10)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .Where(c => c.BranchId == branchId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Case>> SearchCasesAsync(string searchTerm, int? branchId = null)
    {
        var query = _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.AssignedToUser)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(c => c.BranchId == branchId.Value);
        }

        query = query.Where(c =>
            c.CaseNumber.Contains(searchTerm) ||
            c.Title.Contains(searchTerm) ||
            c.Patient.FullNameEn.Contains(searchTerm) ||
            c.Patient.FullNameAr.Contains(searchTerm) ||
            (c.Description != null && c.Description.Contains(searchTerm)));

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
}
