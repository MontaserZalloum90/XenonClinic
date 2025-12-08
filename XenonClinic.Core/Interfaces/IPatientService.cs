using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Patient management
/// </summary>
public interface IPatientService
{
    // Patient Management
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientByEmiratesIdAsync(string emiratesId, int branchId);
    Task<IEnumerable<Patient>> GetPatientsByBranchIdAsync(int branchId);
    Task<IEnumerable<Patient>> SearchPatientsAsync(int branchId, string searchTerm);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task UpdatePatientAsync(Patient patient);
    Task DeletePatientAsync(int id);

    // Patient Medical History
    Task<PatientMedicalHistory?> GetPatientMedicalHistoryAsync(int patientId);
    Task<PatientMedicalHistory> CreateOrUpdateMedicalHistoryAsync(PatientMedicalHistory medicalHistory);

    // Patient Documents
    Task<PatientDocument?> GetDocumentByIdAsync(int id);
    Task<IEnumerable<PatientDocument>> GetPatientDocumentsAsync(int patientId);
    Task<PatientDocument> UploadDocumentAsync(PatientDocument document);
    Task DeleteDocumentAsync(int id);

    // Case Management
    Task<Case?> GetCaseByIdAsync(int id);
    Task<Case?> GetCaseByCaseNumberAsync(string caseNumber);
    Task<IEnumerable<Case>> GetCasesByPatientIdAsync(int patientId);
    Task<IEnumerable<Case>> GetCasesByBranchIdAsync(int branchId);
    Task<IEnumerable<Case>> GetCasesByStatusAsync(int branchId, int statusId);
    Task<IEnumerable<Case>> GetCasesByAssignedUserAsync(string userId);
    Task<IEnumerable<Case>> GetOpenCasesAsync(int branchId);
    Task<IEnumerable<Case>> GetOverdueCasesAsync(int branchId);
    Task<Case> CreateCaseAsync(Case caseEntity);
    Task UpdateCaseAsync(Case caseEntity);
    Task DeleteCaseAsync(int id);
    Task<string> GenerateCaseNumberAsync(int branchId);
    Task CloseCaseAsync(int caseId, string closedBy, string? resolution);

    // Case Notes
    Task<CaseNote?> GetCaseNoteByIdAsync(int id);
    Task<IEnumerable<CaseNote>> GetCaseNotesAsync(int caseId);
    Task<CaseNote> AddCaseNoteAsync(CaseNote note);
    Task UpdateCaseNoteAsync(CaseNote note);
    Task DeleteCaseNoteAsync(int id);

    // Case Activities
    Task<CaseActivity?> GetCaseActivityByIdAsync(int id);
    Task<IEnumerable<CaseActivity>> GetCaseActivitiesAsync(int caseId);
    Task<CaseActivity> AddCaseActivityAsync(CaseActivity activity);

    // Statistics & Reporting
    Task<int> GetTotalPatientsCountAsync(int branchId);
    Task<int> GetNewPatientsCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<int> GetActiveCasesCountAsync(int branchId);
    Task<int> GetOverdueCasesCountAsync(int branchId);
    Task<Dictionary<int, int>> GetCasesByStatusDistributionAsync(int branchId);
    Task<Dictionary<int, int>> GetCasesByTypeDistributionAsync(int branchId);
    Task<Dictionary<string, int>> GetPatientsByGenderDistributionAsync(int branchId);
}
