using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Patient management.
/// Note: Case management has been moved to ICaseService.
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

    // Patient Statistics
    Task<int> GetTotalPatientsCountAsync(int branchId);
    Task<int> GetNewPatientsCountAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetPatientsByGenderDistributionAsync(int branchId);
}
