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

    #region Patient Statistics

    public async Task<int> GetTotalPatientsCountAsync(int branchId)
    {
        return await _context.Patients
            .CountAsync(p => p.BranchId == branchId);
    }

    public async Task<int> GetNewPatientsCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Patients
            .CountAsync(p => p.BranchId == branchId &&
                        p.CreatedAt >= startDate &&
                        p.CreatedAt <= endDate);
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
