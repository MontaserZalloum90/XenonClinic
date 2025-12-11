using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for clinical visit management across all specialties.
/// </summary>
public class ClinicalVisitService : IClinicalVisitService
{
    private readonly ClinicDbContext _context;

    public ClinicalVisitService(ClinicDbContext context)
    {
        _context = context;
    }

    #region Audiology Visits

    public async Task<AudiologyVisit?> GetAudiologyVisitByIdAsync(int id)
    {
        return await _context.AudiologyVisits
            .Include(v => v.Patient)
            .Include(v => v.Branch)
            .Include(v => v.Audiogram)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<AudiologyVisit>> GetAudiologyVisitsByPatientAsync(int patientId, int branchId)
    {
        return await _context.AudiologyVisits
            .Where(v => v.PatientId == patientId && v.BranchId == branchId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AudiologyVisit>> GetAudiologyVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.AudiologyVisits
            .Include(v => v.Patient)
            .Where(v => v.BranchId == branchId);

        if (fromDate.HasValue)
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(v => v.VisitDate <= toDate.Value);

        return await query.OrderByDescending(v => v.VisitDate).ToListAsync();
    }

    public async Task<AudiologyVisit> CreateAudiologyVisitAsync(AudiologyVisit visit)
    {
        visit.CreatedAt = DateTime.UtcNow;
        _context.AudiologyVisits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task UpdateAudiologyVisitAsync(AudiologyVisit visit)
    {
        visit.UpdatedAt = DateTime.UtcNow;
        _context.AudiologyVisits.Update(visit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAudiologyVisitAsync(int id, string? deletedBy = null)
    {
        var visit = await _context.AudiologyVisits.FindAsync(id);
        if (visit != null)
        {
            _context.AudiologyVisits.Remove(visit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Audiogram?> GetAudiogramByIdAsync(int id)
    {
        return await _context.Audiograms
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Audiogram?> GetAudiogramByVisitIdAsync(int visitId)
    {
        var visit = await _context.AudiologyVisits
            .Include(v => v.Audiogram)
            .FirstOrDefaultAsync(v => v.Id == visitId);
        return visit?.Audiogram;
    }

    public async Task<Audiogram> CreateAudiogramAsync(Audiogram audiogram)
    {
        audiogram.CreatedAt = DateTime.UtcNow;
        _context.Audiograms.Add(audiogram);
        await _context.SaveChangesAsync();
        return audiogram;
    }

    #endregion

    #region Dental Visits

    public async Task<DentalVisit?> GetDentalVisitByIdAsync(int id)
    {
        return await _context.DentalVisits
            .Include(v => v.Patient)
            .Include(v => v.Branch)
            .Include(v => v.Procedures)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<DentalVisit>> GetDentalVisitsByPatientAsync(int patientId, int branchId)
    {
        return await _context.DentalVisits
            .Include(v => v.Procedures)
            .Where(v => v.PatientId == patientId && v.BranchId == branchId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DentalVisit>> GetDentalVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.DentalVisits
            .Include(v => v.Patient)
            .Where(v => v.BranchId == branchId);

        if (fromDate.HasValue)
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(v => v.VisitDate <= toDate.Value);

        return await query.OrderByDescending(v => v.VisitDate).ToListAsync();
    }

    public async Task<DentalVisit> CreateDentalVisitAsync(DentalVisit visit)
    {
        visit.CreatedAt = DateTime.UtcNow;
        _context.DentalVisits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task UpdateDentalVisitAsync(DentalVisit visit)
    {
        visit.UpdatedAt = DateTime.UtcNow;
        _context.DentalVisits.Update(visit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDentalVisitAsync(int id, string? deletedBy = null)
    {
        var visit = await _context.DentalVisits.FindAsync(id);
        if (visit != null)
        {
            _context.DentalVisits.Remove(visit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DentalProcedure>> GetDentalProceduresByVisitAsync(int visitId)
    {
        return await _context.DentalProcedures
            .Where(p => p.DentalVisitId == visitId)
            .ToListAsync();
    }

    public async Task AddDentalProcedureAsync(DentalProcedure procedure)
    {
        procedure.CreatedAt = DateTime.UtcNow;
        _context.DentalProcedures.Add(procedure);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Cardiology Visits

    public async Task<CardioVisit?> GetCardioVisitByIdAsync(int id)
    {
        return await _context.CardioVisits
            .Include(v => v.Patient)
            .Include(v => v.Branch)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<CardioVisit>> GetCardioVisitsByPatientAsync(int patientId, int branchId)
    {
        return await _context.CardioVisits
            .Where(v => v.PatientId == patientId && v.BranchId == branchId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CardioVisit>> GetCardioVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.CardioVisits
            .Include(v => v.Patient)
            .Where(v => v.BranchId == branchId);

        if (fromDate.HasValue)
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(v => v.VisitDate <= toDate.Value);

        return await query.OrderByDescending(v => v.VisitDate).ToListAsync();
    }

    public async Task<CardioVisit> CreateCardioVisitAsync(CardioVisit visit)
    {
        visit.CreatedAt = DateTime.UtcNow;
        _context.CardioVisits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task UpdateCardioVisitAsync(CardioVisit visit)
    {
        visit.UpdatedAt = DateTime.UtcNow;
        _context.CardioVisits.Update(visit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCardioVisitAsync(int id, string? deletedBy = null)
    {
        var visit = await _context.CardioVisits.FindAsync(id);
        if (visit != null)
        {
            _context.CardioVisits.Remove(visit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ECGRecord?> GetECGByIdAsync(int id)
    {
        return await _context.ECGRecords
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<ECGRecord>> GetECGsByPatientAsync(int patientId, int branchId)
    {
        return await _context.ECGRecords
            .Where(e => e.PatientId == patientId && e.BranchId == branchId)
            .OrderByDescending(e => e.RecordDate)
            .ToListAsync();
    }

    public async Task<ECGRecord> CreateECGAsync(ECGRecord ecg)
    {
        ecg.CreatedAt = DateTime.UtcNow;
        _context.ECGRecords.Add(ecg);
        await _context.SaveChangesAsync();
        return ecg;
    }

    public async Task<EchoResult?> GetEchoByIdAsync(int id)
    {
        return await _context.EchoResults
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<EchoResult>> GetEchosByPatientAsync(int patientId, int branchId)
    {
        return await _context.EchoResults
            .Where(e => e.PatientId == patientId && e.BranchId == branchId)
            .OrderByDescending(e => e.StudyDate)
            .ToListAsync();
    }

    public async Task<EchoResult> CreateEchoAsync(EchoResult echo)
    {
        echo.CreatedAt = DateTime.UtcNow;
        _context.EchoResults.Add(echo);
        await _context.SaveChangesAsync();
        return echo;
    }

    #endregion

    #region Ophthalmology Visits

    public async Task<OphthalmologyVisit?> GetOphthalmologyVisitByIdAsync(int id)
    {
        return await _context.OphthalmologyVisits
            .Include(v => v.Patient)
            .Include(v => v.Branch)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<OphthalmologyVisit>> GetOphthalmologyVisitsByPatientAsync(int patientId, int branchId)
    {
        return await _context.OphthalmologyVisits
            .Where(v => v.PatientId == patientId && v.BranchId == branchId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<OphthalmologyVisit>> GetOphthalmologyVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.OphthalmologyVisits
            .Include(v => v.Patient)
            .Where(v => v.BranchId == branchId);

        if (fromDate.HasValue)
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(v => v.VisitDate <= toDate.Value);

        return await query.OrderByDescending(v => v.VisitDate).ToListAsync();
    }

    public async Task<OphthalmologyVisit> CreateOphthalmologyVisitAsync(OphthalmologyVisit visit)
    {
        visit.CreatedAt = DateTime.UtcNow;
        _context.OphthalmologyVisits.Add(visit);
        await _context.SaveChangesAsync();
        return visit;
    }

    public async Task UpdateOphthalmologyVisitAsync(OphthalmologyVisit visit)
    {
        visit.UpdatedAt = DateTime.UtcNow;
        _context.OphthalmologyVisits.Update(visit);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOphthalmologyVisitAsync(int id, string? deletedBy = null)
    {
        var visit = await _context.OphthalmologyVisits.FindAsync(id);
        if (visit != null)
        {
            _context.OphthalmologyVisits.Remove(visit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EyePrescription?> GetEyePrescriptionByIdAsync(int id)
    {
        return await _context.Set<EyePrescription>()
            .Include(p => p.Patient)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<EyePrescription>> GetEyePrescriptionsByPatientAsync(int patientId, int branchId)
    {
        return await _context.Set<EyePrescription>()
            .Where(p => p.PatientId == patientId && p.BranchId == branchId && p.IsActive)
            .OrderByDescending(p => p.PrescriptionDate)
            .ToListAsync();
    }

    public async Task<EyePrescription> CreateEyePrescriptionAsync(EyePrescription prescription)
    {
        prescription.CreatedAt = DateTime.UtcNow;
        _context.Set<EyePrescription>().Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    #endregion

    #region Physiotherapy Sessions

    public async Task<PhysioSession?> GetPhysioSessionByIdAsync(int id)
    {
        return await _context.PhysioSessions
            .Include(s => s.Patient)
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<PhysioSession>> GetPhysioSessionsByPatientAsync(int patientId, int branchId)
    {
        return await _context.PhysioSessions
            .Where(s => s.PatientId == patientId && s.BranchId == branchId)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysioSession>> GetPhysioSessionsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.PhysioSessions
            .Include(s => s.Patient)
            .Where(s => s.BranchId == branchId);

        if (fromDate.HasValue)
            query = query.Where(s => s.SessionDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(s => s.SessionDate <= toDate.Value);

        return await query.OrderByDescending(s => s.SessionDate).ToListAsync();
    }

    public async Task<PhysioSession> CreatePhysioSessionAsync(PhysioSession session)
    {
        session.CreatedAt = DateTime.UtcNow;
        _context.PhysioSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task UpdatePhysioSessionAsync(PhysioSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        _context.PhysioSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePhysioSessionAsync(int id, string? deletedBy = null)
    {
        var session = await _context.PhysioSessions.FindAsync(id);
        if (session != null)
        {
            _context.PhysioSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PhysioAssessment?> GetPhysioAssessmentByIdAsync(int id)
    {
        return await _context.Set<PhysioAssessment>()
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<PhysioAssessment>> GetPhysioAssessmentsByPatientAsync(int patientId, int branchId)
    {
        return await _context.Set<PhysioAssessment>()
            .Where(a => a.PatientId == patientId && a.BranchId == branchId)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync();
    }

    public async Task<PhysioAssessment> CreatePhysioAssessmentAsync(PhysioAssessment assessment)
    {
        assessment.CreatedAt = DateTime.UtcNow;
        _context.Set<PhysioAssessment>().Add(assessment);
        await _context.SaveChangesAsync();
        return assessment;
    }

    #endregion

    #region Statistics

    public async Task<ClinicalVisitStatisticsDto> GetStatisticsAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var stats = new ClinicalVisitStatisticsDto();

        // Get visits by specialty
        stats.VisitsBySpecialty = await GetVisitsBySpecialtyAsync(branchId, fromDate, toDate);
        stats.TotalVisits = stats.VisitsBySpecialty.Values.Sum();

        // Today's visits
        stats.VisitsToday = await CountAllVisitsAsync(branchId, today, today.AddDays(1));

        // This week's visits
        stats.VisitsThisWeek = await CountAllVisitsAsync(branchId, weekStart, weekStart.AddDays(7));

        // This month's visits
        stats.VisitsThisMonth = await CountAllVisitsAsync(branchId, monthStart, monthStart.AddMonths(1));

        return stats;
    }

    public async Task<int> GetTotalVisitsCountAsync(int branchId, string specialtyType)
    {
        return specialtyType.ToLowerInvariant() switch
        {
            "audiology" => await _context.AudiologyVisits.CountAsync(v => v.BranchId == branchId),
            "dental" => await _context.DentalVisits.CountAsync(v => v.BranchId == branchId),
            "cardiology" => await _context.CardioVisits.CountAsync(v => v.BranchId == branchId),
            "ophthalmology" => await _context.OphthalmologyVisits.CountAsync(v => v.BranchId == branchId),
            "physiotherapy" => await _context.PhysioSessions.CountAsync(s => s.BranchId == branchId),
            _ => 0
        };
    }

    public async Task<Dictionary<string, int>> GetVisitsBySpecialtyAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var result = new Dictionary<string, int>();

        // Audiology
        var audiologyQuery = _context.AudiologyVisits.Where(v => v.BranchId == branchId);
        if (fromDate.HasValue) audiologyQuery = audiologyQuery.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue) audiologyQuery = audiologyQuery.Where(v => v.VisitDate <= toDate.Value);
        result["Audiology"] = await audiologyQuery.CountAsync();

        // Dental
        var dentalQuery = _context.DentalVisits.Where(v => v.BranchId == branchId);
        if (fromDate.HasValue) dentalQuery = dentalQuery.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue) dentalQuery = dentalQuery.Where(v => v.VisitDate <= toDate.Value);
        result["Dental"] = await dentalQuery.CountAsync();

        // Cardiology
        var cardioQuery = _context.CardioVisits.Where(v => v.BranchId == branchId);
        if (fromDate.HasValue) cardioQuery = cardioQuery.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue) cardioQuery = cardioQuery.Where(v => v.VisitDate <= toDate.Value);
        result["Cardiology"] = await cardioQuery.CountAsync();

        // Ophthalmology
        var ophthQuery = _context.OphthalmologyVisits.Where(v => v.BranchId == branchId);
        if (fromDate.HasValue) ophthQuery = ophthQuery.Where(v => v.VisitDate >= fromDate.Value);
        if (toDate.HasValue) ophthQuery = ophthQuery.Where(v => v.VisitDate <= toDate.Value);
        result["Ophthalmology"] = await ophthQuery.CountAsync();

        // Physiotherapy
        var physioQuery = _context.PhysioSessions.Where(s => s.BranchId == branchId);
        if (fromDate.HasValue) physioQuery = physioQuery.Where(s => s.SessionDate >= fromDate.Value);
        if (toDate.HasValue) physioQuery = physioQuery.Where(s => s.SessionDate <= toDate.Value);
        result["Physiotherapy"] = await physioQuery.CountAsync();

        return result;
    }

    private async Task<int> CountAllVisitsAsync(int branchId, DateTime from, DateTime to)
    {
        var count = 0;
        count += await _context.AudiologyVisits.CountAsync(v => v.BranchId == branchId && v.VisitDate >= from && v.VisitDate < to);
        count += await _context.DentalVisits.CountAsync(v => v.BranchId == branchId && v.VisitDate >= from && v.VisitDate < to);
        count += await _context.CardioVisits.CountAsync(v => v.BranchId == branchId && v.VisitDate >= from && v.VisitDate < to);
        count += await _context.OphthalmologyVisits.CountAsync(v => v.BranchId == branchId && v.VisitDate >= from && v.VisitDate < to);
        count += await _context.PhysioSessions.CountAsync(s => s.BranchId == branchId && s.SessionDate >= from && s.SessionDate < to);
        return count;
    }

    #endregion
}
