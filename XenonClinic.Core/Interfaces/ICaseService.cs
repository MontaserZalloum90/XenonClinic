using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for managing patient cases
/// </summary>
public interface ICaseService
{
    // Case CRUD operations
    Task<Case> CreateCaseAsync(Case caseEntity);
    Task<Case> UpdateCaseAsync(Case caseEntity);
    Task<Case?> GetCaseByIdAsync(int caseId);
    Task<Case?> GetCaseByNumberAsync(string caseNumber);
    Task<List<Case>> GetCasesByPatientIdAsync(int patientId);
    Task<List<Case>> GetCasesByBranchIdAsync(int branchId, bool includeInactive = false);
    Task<List<Case>> GetCasesByAssignedUserAsync(string userId);
    Task<List<Case>> GetCasesByStatusAsync(int statusId);
    Task<bool> DeleteCaseAsync(int caseId);

    // Case status management
    Task<bool> ChangeCaseStatusAsync(int caseId, int newStatusId, string? notes = null);
    Task<bool> CloseCaseAsync(int caseId, string resolution, string closedBy);
    Task<bool> ReopenCaseAsync(int caseId);

    // Case assignment
    Task<bool> AssignCaseAsync(int caseId, string userId);
    Task<bool> UnassignCaseAsync(int caseId);

    // Case notes
    Task<CaseNote> AddCaseNoteAsync(CaseNote note);
    Task<CaseNote> UpdateCaseNoteAsync(CaseNote note);
    Task<List<CaseNote>> GetCaseNotesAsync(int caseId);
    Task<bool> DeleteCaseNoteAsync(int noteId);

    // Case activities
    Task<CaseActivity> AddCaseActivityAsync(CaseActivity activity);
    Task<CaseActivity> UpdateCaseActivityAsync(CaseActivity activity);
    Task<List<CaseActivity>> GetCaseActivitiesAsync(int caseId);
    Task<List<CaseActivity>> GetPendingActivitiesAsync(int caseId);
    Task<List<CaseActivity>> GetOverdueActivitiesAsync(int caseId);
    Task<bool> CompleteCaseActivityAsync(int activityId, string? result = null);
    Task<bool> DeleteCaseActivityAsync(int activityId);

    // Case types and statuses
    Task<List<CaseType>> GetCaseTypesAsync(int tenantId);
    Task<List<CaseStatus>> GetCaseStatusesAsync(int tenantId);
    Task<CaseType> CreateCaseTypeAsync(CaseType caseType);
    Task<CaseStatus> CreateCaseStatusAsync(CaseStatus caseStatus);

    // Case number generation
    Task<string> GenerateCaseNumberAsync(int branchId);

    // Dashboard and statistics
    Task<Dictionary<string, int>> GetCaseStatisticsByBranchAsync(int branchId);
    Task<List<Case>> GetRecentCasesAsync(int branchId, int count = 10);
    Task<List<Case>> SearchCasesAsync(string searchTerm, int? branchId = null);
}
