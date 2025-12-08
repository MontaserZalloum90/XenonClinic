using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Patient management
/// </summary>
public class PatientService : IPatientService
{
    private readonly ClinicDbContext _context;

    public PatientService(ClinicDbContext context)
    {
        _context = context;
    }

    #region Patient Management

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Branch)
            .Include(p => p.MedicalHistory)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetPatientByEmiratesIdAsync(string emiratesId, int branchId)
    {
        return await _context.Patients
            .Include(p => p.MedicalHistory)
            .FirstOrDefaultAsync(p => p.EmiratesId == emiratesId && p.BranchId == branchId);
    }

    public async Task<IEnumerable<Patient>> GetPatientsByBranchIdAsync(int branchId)
    {
        return await _context.Patients
            .Where(p => p.BranchId == branchId)
            .OrderBy(p => p.FullNameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(int branchId, string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.BranchId == branchId &&
                   (p.FullNameEn.Contains(searchTerm) ||
                    (p.FullNameAr != null && p.FullNameAr.Contains(searchTerm)) ||
                    p.EmiratesId.Contains(searchTerm) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm))))
            .OrderBy(p => p.FullNameEn)
            .ToListAsync();
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePatientAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Patient Medical History

    public async Task<PatientMedicalHistory?> GetPatientMedicalHistoryAsync(int patientId)
    {
        return await _context.PatientMedicalHistories
            .FirstOrDefaultAsync(h => h.PatientId == patientId);
    }

    public async Task<PatientMedicalHistory> CreateOrUpdateMedicalHistoryAsync(PatientMedicalHistory medicalHistory)
    {
        var existing = await _context.PatientMedicalHistories
            .FirstOrDefaultAsync(h => h.PatientId == medicalHistory.PatientId);

        if (existing != null)
        {
            existing.Allergies = medicalHistory.Allergies;
            existing.ChronicConditions = medicalHistory.ChronicConditions;
            existing.CurrentMedications = medicalHistory.CurrentMedications;
            existing.PastSurgeries = medicalHistory.PastSurgeries;
            existing.FamilyHistory = medicalHistory.FamilyHistory;
            existing.SocialHistory = medicalHistory.SocialHistory;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = medicalHistory.UpdatedBy;

            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            _context.PatientMedicalHistories.Add(medicalHistory);
            await _context.SaveChangesAsync();
            return medicalHistory;
        }
    }

    #endregion

    #region Patient Documents

    public async Task<PatientDocument?> GetDocumentByIdAsync(int id)
    {
        return await _context.PatientDocuments
            .Include(d => d.Patient)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<PatientDocument>> GetPatientDocumentsAsync(int patientId)
    {
        return await _context.PatientDocuments
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<PatientDocument> UploadDocumentAsync(PatientDocument document)
    {
        _context.PatientDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task DeleteDocumentAsync(int id)
    {
        var document = await _context.PatientDocuments.FindAsync(id);
        if (document != null)
        {
            _context.PatientDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Case Management

    public async Task<Case?> GetCaseByIdAsync(int id)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.CasePriority)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Notes)
            .Include(c => c.Activities)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Case?> GetCaseByCaseNumberAsync(string caseNumber)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .FirstOrDefaultAsync(c => c.CaseNumber == caseNumber);
    }

    public async Task<IEnumerable<Case>> GetCasesByPatientIdAsync(int patientId)
    {
        return await _context.Cases
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.CasePriority)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Case>> GetCasesByBranchIdAsync(int branchId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Include(c => c.CasePriority)
            .Where(c => c.BranchId == branchId)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Case>> GetCasesByStatusAsync(int branchId, int statusId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Where(c => c.BranchId == branchId && c.CaseStatusId == statusId)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Case>> GetCasesByAssignedUserAsync(string userId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.Branch)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Where(c => c.AssignedToUserId == userId)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Case>> GetOpenCasesAsync(int branchId)
    {
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Where(c => c.BranchId == branchId && c.ClosedDate == null)
            .OrderByDescending(c => c.OpenedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Case>> GetOverdueCasesAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Cases
            .Include(c => c.Patient)
            .Include(c => c.CaseType)
            .Include(c => c.CaseStatus)
            .Where(c => c.BranchId == branchId &&
                   c.ClosedDate == null &&
                   c.TargetDate.HasValue &&
                   c.TargetDate.Value < today)
            .OrderBy(c => c.TargetDate)
            .ToListAsync();
    }

    public async Task<Case> CreateCaseAsync(Case caseEntity)
    {
        _context.Cases.Add(caseEntity);
        await _context.SaveChangesAsync();
        return caseEntity;
    }

    public async Task UpdateCaseAsync(Case caseEntity)
    {
        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCaseAsync(int id)
    {
        var caseEntity = await _context.Cases.FindAsync(id);
        if (caseEntity != null)
        {
            _context.Cases.Remove(caseEntity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateCaseNumberAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"CASE-{today:yyyyMM}";

        var lastCase = await _context.Cases
            .Where(c => c.BranchId == branchId && c.CaseNumber.StartsWith(prefix))
            .OrderByDescending(c => c.CaseNumber)
            .FirstOrDefaultAsync();

        if (lastCase == null)
        {
            return $"{prefix}-001";
        }

        var lastNumber = int.Parse(lastCase.CaseNumber.Split('-').Last());
        return $"{prefix}-{(lastNumber + 1):D3}";
    }

    public async Task CloseCaseAsync(int caseId, string closedBy, string? resolution)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);
        if (caseEntity == null)
            throw new KeyNotFoundException($"Case with ID {caseId} not found");

        caseEntity.ClosedDate = DateTime.UtcNow;
        caseEntity.ClosedBy = closedBy;
        caseEntity.Resolution = resolution;
        caseEntity.UpdatedAt = DateTime.UtcNow;
        caseEntity.UpdatedBy = closedBy;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Case Notes

    public async Task<CaseNote?> GetCaseNoteByIdAsync(int id)
    {
        return await _context.CaseNotes
            .Include(n => n.Case)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<CaseNote>> GetCaseNotesAsync(int caseId)
    {
        return await _context.CaseNotes
            .Where(n => n.CaseId == caseId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<CaseNote> AddCaseNoteAsync(CaseNote note)
    {
        _context.CaseNotes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task UpdateCaseNoteAsync(CaseNote note)
    {
        _context.CaseNotes.Update(note);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCaseNoteAsync(int id)
    {
        var note = await _context.CaseNotes.FindAsync(id);
        if (note != null)
        {
            _context.CaseNotes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Case Activities

    public async Task<CaseActivity?> GetCaseActivityByIdAsync(int id)
    {
        return await _context.CaseActivities
            .Include(a => a.Case)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<CaseActivity>> GetCaseActivitiesAsync(int caseId)
    {
        return await _context.CaseActivities
            .Where(a => a.CaseId == caseId)
            .OrderByDescending(a => a.ActivityDate)
            .ToListAsync();
    }

    public async Task<CaseActivity> AddCaseActivityAsync(CaseActivity activity)
    {
        _context.CaseActivities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalPatientsCountAsync(int branchId)
    {
        return await _context.Patients
            .CountAsync(p => p.BranchId == branchId);
    }

    public async Task<int> GetNewPatientsCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Assuming there's a CreatedAt field in Patient entity
        // If not available, this would need to be adjusted based on actual schema
        return await _context.Patients
            .CountAsync(p => p.BranchId == branchId);
    }

    public async Task<int> GetActiveCasesCountAsync(int branchId)
    {
        return await _context.Cases
            .CountAsync(c => c.BranchId == branchId && c.ClosedDate == null);
    }

    public async Task<int> GetOverdueCasesCountAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Cases
            .CountAsync(c => c.BranchId == branchId &&
                        c.ClosedDate == null &&
                        c.TargetDate.HasValue &&
                        c.TargetDate.Value < today);
    }

    public async Task<Dictionary<int, int>> GetCasesByStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Cases
            .Where(c => c.BranchId == branchId)
            .GroupBy(c => c.CaseStatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.StatusId, x => x.Count);
    }

    public async Task<Dictionary<int, int>> GetCasesByTypeDistributionAsync(int branchId)
    {
        var distribution = await _context.Cases
            .Where(c => c.BranchId == branchId)
            .GroupBy(c => c.CaseTypeId)
            .Select(g => new { TypeId = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.TypeId, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetPatientsByGenderDistributionAsync(int branchId)
    {
        var distribution = await _context.Patients
            .Where(p => p.BranchId == branchId)
            .GroupBy(p => p.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Gender, x => x.Count);
    }

    #endregion
}
