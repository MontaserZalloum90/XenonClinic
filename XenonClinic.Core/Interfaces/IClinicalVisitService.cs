using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Physiotherapy;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for clinical visit management across all specialties.
/// </summary>
public interface IClinicalVisitService
{
    #region Audiology Visits

    Task<AudiologyVisit?> GetAudiologyVisitByIdAsync(int id);
    Task<IEnumerable<AudiologyVisit>> GetAudiologyVisitsByPatientAsync(int patientId, int branchId);
    Task<IEnumerable<AudiologyVisit>> GetAudiologyVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<AudiologyVisit> CreateAudiologyVisitAsync(AudiologyVisit visit);
    Task UpdateAudiologyVisitAsync(AudiologyVisit visit);
    Task DeleteAudiologyVisitAsync(int id, string? deletedBy = null);
    Task<Audiogram?> GetAudiogramByIdAsync(int id);
    Task<Audiogram?> GetAudiogramByVisitIdAsync(int visitId);
    Task<Audiogram> CreateAudiogramAsync(Audiogram audiogram);

    #endregion

    #region Dental Visits

    Task<DentalVisit?> GetDentalVisitByIdAsync(int id);
    Task<IEnumerable<DentalVisit>> GetDentalVisitsByPatientAsync(int patientId, int branchId);
    Task<IEnumerable<DentalVisit>> GetDentalVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<DentalVisit> CreateDentalVisitAsync(DentalVisit visit);
    Task UpdateDentalVisitAsync(DentalVisit visit);
    Task DeleteDentalVisitAsync(int id, string? deletedBy = null);
    Task<IEnumerable<DentalProcedure>> GetDentalProceduresByVisitAsync(int visitId);
    Task AddDentalProcedureAsync(DentalProcedure procedure);

    #endregion

    #region Cardiology Visits

    Task<CardioVisit?> GetCardioVisitByIdAsync(int id);
    Task<IEnumerable<CardioVisit>> GetCardioVisitsByPatientAsync(int patientId, int branchId);
    Task<IEnumerable<CardioVisit>> GetCardioVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<CardioVisit> CreateCardioVisitAsync(CardioVisit visit);
    Task UpdateCardioVisitAsync(CardioVisit visit);
    Task DeleteCardioVisitAsync(int id, string? deletedBy = null);
    Task<ECGRecord?> GetECGByIdAsync(int id);
    Task<IEnumerable<ECGRecord>> GetECGsByPatientAsync(int patientId, int branchId);
    Task<ECGRecord> CreateECGAsync(ECGRecord ecg);
    Task<EchoResult?> GetEchoByIdAsync(int id);
    Task<IEnumerable<EchoResult>> GetEchosByPatientAsync(int patientId, int branchId);
    Task<EchoResult> CreateEchoAsync(EchoResult echo);

    #endregion

    #region Ophthalmology Visits

    Task<OphthalmologyVisit?> GetOphthalmologyVisitByIdAsync(int id);
    Task<IEnumerable<OphthalmologyVisit>> GetOphthalmologyVisitsByPatientAsync(int patientId, int branchId);
    Task<IEnumerable<OphthalmologyVisit>> GetOphthalmologyVisitsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<OphthalmologyVisit> CreateOphthalmologyVisitAsync(OphthalmologyVisit visit);
    Task UpdateOphthalmologyVisitAsync(OphthalmologyVisit visit);
    Task DeleteOphthalmologyVisitAsync(int id, string? deletedBy = null);
    Task<EyePrescription?> GetEyePrescriptionByIdAsync(int id);
    Task<IEnumerable<EyePrescription>> GetEyePrescriptionsByPatientAsync(int patientId, int branchId);
    Task<EyePrescription> CreateEyePrescriptionAsync(EyePrescription prescription);

    #endregion

    #region Physiotherapy Sessions

    Task<PhysioSession?> GetPhysioSessionByIdAsync(int id);
    Task<IEnumerable<PhysioSession>> GetPhysioSessionsByPatientAsync(int patientId, int branchId);
    Task<IEnumerable<PhysioSession>> GetPhysioSessionsByBranchAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<PhysioSession> CreatePhysioSessionAsync(PhysioSession session);
    Task UpdatePhysioSessionAsync(PhysioSession session);
    Task DeletePhysioSessionAsync(int id, string? deletedBy = null);
    Task<PhysioAssessment?> GetPhysioAssessmentByIdAsync(int id);
    Task<IEnumerable<PhysioAssessment>> GetPhysioAssessmentsByPatientAsync(int patientId, int branchId);
    Task<PhysioAssessment> CreatePhysioAssessmentAsync(PhysioAssessment assessment);

    #endregion

    #region Statistics

    Task<ClinicalVisitStatisticsDto> GetStatisticsAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalVisitsCountAsync(int branchId, string specialtyType);
    Task<Dictionary<string, int>> GetVisitsBySpecialtyAsync(int branchId, DateTime? fromDate = null, DateTime? toDate = null);

    #endregion
}
