using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for clinical visit management across all specialties.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClinicalVisitsController : BaseApiController
{
    private readonly IClinicalVisitService _clinicalVisitService;
    private readonly ICurrentUserContext _userContext;
    private readonly IPatientService _patientService;
    private readonly ILogger<ClinicalVisitsController> _logger;

    public ClinicalVisitsController(
        IClinicalVisitService clinicalVisitService,
        ICurrentUserContext userContext,
        IPatientService patientService,
        ILogger<ClinicalVisitsController> logger)
    {
        _clinicalVisitService = clinicalVisitService;
        _userContext = userContext;
        _patientService = patientService;
        _logger = logger;
    }

    #region Audiology Endpoints

    /// <summary>
    /// Gets an audiology visit by ID.
    /// </summary>
    [HttpGet("audiology/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<AudiologyVisitDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<AudiologyVisitDto>>> GetAudiologyVisit(int id)
    {
        var visit = await _clinicalVisitService.GetAudiologyVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        return ApiOk(MapToAudiologyVisitDto(visit));
    }

    /// <summary>
    /// Gets audiology visits for a patient.
    /// </summary>
    [HttpGet("audiology/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AudiologyVisitDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AudiologyVisitDto>>>> GetAudiologyVisitsByPatient(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var visits = await _clinicalVisitService.GetAudiologyVisitsByPatientAsync(patientId, branchId);
        return ApiOk(visits.Select(MapToAudiologyVisitDto));
    }

    /// <summary>
    /// Creates a new audiology visit.
    /// </summary>
    [HttpPost("audiology")]
    [ProducesResponseType(typeof(ApiResponse<AudiologyVisitDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<AudiologyVisitDto>>> CreateAudiologyVisit(
        [FromBody] CreateAudiologyVisitDto dto,
        [FromServices] IValidator<CreateAudiologyVisitDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));

        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
            return ApiBadRequest(ClinicalVisitValidationMessages.PatientNotFound);

        var visit = new AudiologyVisit
        {
            PatientId = dto.PatientId,
            BranchId = _userContext.BranchId ?? patient.BranchId,
            VisitDate = dto.VisitDate,
            ChiefComplaint = dto.ChiefComplaint,
            ProviderId = dto.ProviderId?.ToString(),
            Notes = dto.Notes,
            CreatedBy = _userContext.UserId
        };

        var created = await _clinicalVisitService.CreateAudiologyVisitAsync(visit);
        _logger.LogInformation("Created audiology visit {VisitId} for patient {PatientId}", created.Id, dto.PatientId);

        return ApiCreated(MapToAudiologyVisitDto(created));
    }

    /// <summary>
    /// Deletes an audiology visit.
    /// </summary>
    [HttpDelete("audiology/{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult> DeleteAudiologyVisit(int id)
    {
        var visit = await _clinicalVisitService.GetAudiologyVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        await _clinicalVisitService.DeleteAudiologyVisitAsync(id, _userContext.UserId);
        return NoContent();
    }

    #endregion

    #region Dental Endpoints

    /// <summary>
    /// Gets a dental visit by ID.
    /// </summary>
    [HttpGet("dental/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DentalVisitDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<DentalVisitDto>>> GetDentalVisit(int id)
    {
        var visit = await _clinicalVisitService.GetDentalVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        return ApiOk(MapToDentalVisitDto(visit));
    }

    /// <summary>
    /// Gets dental visits for a patient.
    /// </summary>
    [HttpGet("dental/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DentalVisitDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DentalVisitDto>>>> GetDentalVisitsByPatient(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var visits = await _clinicalVisitService.GetDentalVisitsByPatientAsync(patientId, branchId);
        return ApiOk(visits.Select(MapToDentalVisitDto));
    }

    /// <summary>
    /// Creates a new dental visit.
    /// </summary>
    [HttpPost("dental")]
    [ProducesResponseType(typeof(ApiResponse<DentalVisitDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<DentalVisitDto>>> CreateDentalVisit(
        [FromBody] CreateDentalVisitDto dto,
        [FromServices] IValidator<CreateDentalVisitDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));

        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
            return ApiBadRequest(ClinicalVisitValidationMessages.PatientNotFound);

        var visit = new DentalVisit
        {
            PatientId = dto.PatientId,
            BranchId = _userContext.BranchId ?? patient.BranchId,
            VisitDate = dto.VisitDate,
            ChiefComplaint = dto.ChiefComplaint,
            ClinicalFindings = dto.ExaminationFindings,
            Diagnosis = dto.Diagnosis,
            TreatmentNotes = dto.TreatmentProvided,
            ProviderId = dto.ProviderId?.ToString(),
            CreatedBy = _userContext.UserId
        };

        var created = await _clinicalVisitService.CreateDentalVisitAsync(visit);
        _logger.LogInformation("Created dental visit {VisitId} for patient {PatientId}", created.Id, dto.PatientId);

        return ApiCreated(MapToDentalVisitDto(created));
    }

    /// <summary>
    /// Deletes a dental visit.
    /// </summary>
    [HttpDelete("dental/{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult> DeleteDentalVisit(int id)
    {
        var visit = await _clinicalVisitService.GetDentalVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        await _clinicalVisitService.DeleteDentalVisitAsync(id, _userContext.UserId);
        return NoContent();
    }

    #endregion

    #region Cardiology Endpoints

    /// <summary>
    /// Gets a cardiology visit by ID.
    /// </summary>
    [HttpGet("cardiology/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CardioVisitDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<CardioVisitDto>>> GetCardioVisit(int id)
    {
        var visit = await _clinicalVisitService.GetCardioVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        return ApiOk(MapToCardioVisitDto(visit));
    }

    /// <summary>
    /// Gets cardiology visits for a patient.
    /// </summary>
    [HttpGet("cardiology/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CardioVisitDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CardioVisitDto>>>> GetCardioVisitsByPatient(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var visits = await _clinicalVisitService.GetCardioVisitsByPatientAsync(patientId, branchId);
        return ApiOk(visits.Select(MapToCardioVisitDto));
    }

    /// <summary>
    /// Creates a new cardiology visit.
    /// </summary>
    [HttpPost("cardiology")]
    [ProducesResponseType(typeof(ApiResponse<CardioVisitDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<CardioVisitDto>>> CreateCardioVisit(
        [FromBody] CreateCardioVisitDto dto,
        [FromServices] IValidator<CreateCardioVisitDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));

        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
            return ApiBadRequest(ClinicalVisitValidationMessages.PatientNotFound);

        var visit = new CardioVisit
        {
            PatientId = dto.PatientId,
            BranchId = _userContext.BranchId ?? patient.BranchId,
            VisitDate = dto.VisitDate,
            ChiefComplaint = dto.ChiefComplaint,
            Notes = dto.Notes,
            ProviderId = dto.ProviderId?.ToString(),
            CreatedBy = _userContext.UserId
        };

        var created = await _clinicalVisitService.CreateCardioVisitAsync(visit);
        _logger.LogInformation("Created cardiology visit {VisitId} for patient {PatientId}", created.Id, dto.PatientId);

        return ApiCreated(MapToCardioVisitDto(created));
    }

    /// <summary>
    /// Gets ECG records for a patient.
    /// </summary>
    [HttpGet("cardiology/ecg/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ECGRecordDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ECGRecordDto>>>> GetPatientECGs(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var ecgs = await _clinicalVisitService.GetECGsByPatientAsync(patientId, branchId);
        return ApiOk(ecgs.Select(MapToECGRecordDto));
    }

    #endregion

    #region Ophthalmology Endpoints

    /// <summary>
    /// Gets an ophthalmology visit by ID.
    /// </summary>
    [HttpGet("ophthalmology/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OphthalmologyVisitDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<OphthalmologyVisitDto>>> GetOphthalmologyVisit(int id)
    {
        var visit = await _clinicalVisitService.GetOphthalmologyVisitByIdAsync(id);
        if (visit == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (visit.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        return ApiOk(MapToOphthalmologyVisitDto(visit));
    }

    /// <summary>
    /// Gets ophthalmology visits for a patient.
    /// </summary>
    [HttpGet("ophthalmology/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OphthalmologyVisitDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OphthalmologyVisitDto>>>> GetOphthalmologyVisitsByPatient(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var visits = await _clinicalVisitService.GetOphthalmologyVisitsByPatientAsync(patientId, branchId);
        return ApiOk(visits.Select(MapToOphthalmologyVisitDto));
    }

    /// <summary>
    /// Creates a new ophthalmology visit.
    /// </summary>
    [HttpPost("ophthalmology")]
    [ProducesResponseType(typeof(ApiResponse<OphthalmologyVisitDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<OphthalmologyVisitDto>>> CreateOphthalmologyVisit(
        [FromBody] CreateOphthalmologyVisitDto dto,
        [FromServices] IValidator<CreateOphthalmologyVisitDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));

        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
            return ApiBadRequest(ClinicalVisitValidationMessages.PatientNotFound);

        var visit = new OphthalmologyVisit
        {
            PatientId = dto.PatientId,
            BranchId = _userContext.BranchId ?? patient.BranchId,
            VisitDate = dto.VisitDate,
            ChiefComplaint = dto.ChiefComplaint,
            Notes = dto.Notes,
            ProviderId = dto.ProviderId?.ToString(),
            CreatedBy = _userContext.UserId
        };

        var created = await _clinicalVisitService.CreateOphthalmologyVisitAsync(visit);
        _logger.LogInformation("Created ophthalmology visit {VisitId} for patient {PatientId}", created.Id, dto.PatientId);

        return ApiCreated(MapToOphthalmologyVisitDto(created));
    }

    /// <summary>
    /// Gets eye prescriptions for a patient.
    /// </summary>
    [HttpGet("ophthalmology/prescriptions/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EyePrescriptionDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EyePrescriptionDto>>>> GetPatientEyePrescriptions(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var prescriptions = await _clinicalVisitService.GetEyePrescriptionsByPatientAsync(patientId, branchId);
        return ApiOk(prescriptions.Select(MapToEyePrescriptionDto));
    }

    #endregion

    #region Physiotherapy Endpoints

    /// <summary>
    /// Gets a physiotherapy session by ID.
    /// </summary>
    [HttpGet("physiotherapy/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PhysioSessionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse<PhysioSessionDto>>> GetPhysioSession(int id)
    {
        var session = await _clinicalVisitService.GetPhysioSessionByIdAsync(id);
        if (session == null)
            return ApiNotFound(ClinicalVisitValidationMessages.VisitNotFound);

        if (session.BranchId != _userContext.BranchId)
            return ApiForbidden(ClinicalVisitValidationMessages.BranchAccessDenied);

        return ApiOk(MapToPhysioSessionDto(session));
    }

    /// <summary>
    /// Gets physiotherapy sessions for a patient.
    /// </summary>
    [HttpGet("physiotherapy/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhysioSessionDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhysioSessionDto>>>> GetPhysioSessionsByPatient(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var sessions = await _clinicalVisitService.GetPhysioSessionsByPatientAsync(patientId, branchId);
        return ApiOk(sessions.Select(MapToPhysioSessionDto));
    }

    /// <summary>
    /// Creates a new physiotherapy session.
    /// </summary>
    [HttpPost("physiotherapy")]
    [ProducesResponseType(typeof(ApiResponse<PhysioSessionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<ApiResponse<PhysioSessionDto>>> CreatePhysioSession(
        [FromBody] CreatePhysioSessionDto dto,
        [FromServices] IValidator<CreatePhysioSessionDto> validator)
    {
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));

        var patient = await _patientService.GetPatientByIdAsync(dto.PatientId);
        if (patient == null)
            return ApiBadRequest(ClinicalVisitValidationMessages.PatientNotFound);

        var session = new PhysioSession
        {
            PatientId = dto.PatientId,
            BranchId = _userContext.BranchId ?? patient.BranchId,
            SessionDate = dto.VisitDate,
            DurationMinutes = dto.DurationMinutes ?? 30,
            PainLevelBefore = dto.PainLevelBefore,
            ModalitiesUsed = dto.TechniquesUsed,
            PhysiotherapistId = dto.ProviderId?.ToString(),
            Notes = dto.Notes,
            CreatedBy = _userContext.UserId
        };

        var created = await _clinicalVisitService.CreatePhysioSessionAsync(session);
        _logger.LogInformation("Created physiotherapy session {SessionId} for patient {PatientId}", created.Id, dto.PatientId);

        return ApiCreated(MapToPhysioSessionDto(created));
    }

    /// <summary>
    /// Gets physiotherapy assessments for a patient.
    /// </summary>
    [HttpGet("physiotherapy/assessments/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PhysioAssessmentDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhysioAssessmentDto>>>> GetPatientPhysioAssessments(int patientId)
    {
        var branchId = _userContext.BranchId ?? 0;
        var assessments = await _clinicalVisitService.GetPhysioAssessmentsByPatientAsync(patientId, branchId);
        return ApiOk(assessments.Select(MapToPhysioAssessmentDto));
    }

    #endregion

    #region Statistics Endpoints

    /// <summary>
    /// Gets clinical visit statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalVisitStatisticsDto>), 200)]
    public async Task<ActionResult<ApiResponse<ClinicalVisitStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var branchId = _userContext.BranchId ?? 0;
        var stats = await _clinicalVisitService.GetStatisticsAsync(branchId, fromDate, toDate);
        return ApiOk(stats);
    }

    /// <summary>
    /// Gets visit count by specialty.
    /// </summary>
    [HttpGet("statistics/by-specialty")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), 200)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetVisitsBySpecialty(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var branchId = _userContext.BranchId ?? 0;
        var stats = await _clinicalVisitService.GetVisitsBySpecialtyAsync(branchId, fromDate, toDate);
        return ApiOk(stats);
    }

    #endregion

    #region Mapping Methods

    private static AudiologyVisitDto MapToAudiologyVisitDto(AudiologyVisit visit) => new()
    {
        Id = visit.Id,
        PatientId = visit.PatientId,
        PatientName = visit.Patient?.FullNameEn,
        BranchId = visit.BranchId,
        BranchName = visit.Branch?.NameEn,
        ProviderName = visit.ProviderId,
        VisitDate = visit.VisitDate,
        ChiefComplaint = visit.ChiefComplaint,
        Notes = visit.Notes,
        CreatedAt = visit.CreatedAt,
        CreatedBy = visit.CreatedBy,
        UpdatedAt = visit.UpdatedAt,
        UpdatedBy = visit.UpdatedBy
    };

    private static DentalVisitDto MapToDentalVisitDto(DentalVisit visit) => new()
    {
        Id = visit.Id,
        PatientId = visit.PatientId,
        PatientName = visit.Patient?.FullNameEn,
        BranchId = visit.BranchId,
        BranchName = visit.Branch?.NameEn,
        ProviderName = visit.ProviderId,
        VisitDate = visit.VisitDate,
        ChiefComplaint = visit.ChiefComplaint,
        ExaminationFindings = visit.ClinicalFindings,
        Diagnosis = visit.Diagnosis,
        TreatmentProvided = visit.TreatmentNotes,
        CreatedAt = visit.CreatedAt,
        CreatedBy = visit.CreatedBy,
        UpdatedAt = visit.UpdatedAt,
        UpdatedBy = visit.UpdatedBy
    };

    private static CardioVisitDto MapToCardioVisitDto(CardioVisit visit) => new()
    {
        Id = visit.Id,
        PatientId = visit.PatientId,
        PatientName = visit.Patient?.FullNameEn,
        BranchId = visit.BranchId,
        BranchName = visit.Branch?.NameEn,
        ProviderName = visit.ProviderId,
        VisitDate = visit.VisitDate,
        ChiefComplaint = visit.ChiefComplaint,
        Notes = visit.Notes,
        CreatedAt = visit.CreatedAt,
        CreatedBy = visit.CreatedBy,
        UpdatedAt = visit.UpdatedAt,
        UpdatedBy = visit.UpdatedBy
    };

    private static OphthalmologyVisitDto MapToOphthalmologyVisitDto(OphthalmologyVisit visit) => new()
    {
        Id = visit.Id,
        PatientId = visit.PatientId,
        PatientName = visit.Patient?.FullNameEn,
        BranchId = visit.BranchId,
        BranchName = visit.Branch?.NameEn,
        ProviderName = visit.ProviderId,
        VisitDate = visit.VisitDate,
        ChiefComplaint = visit.ChiefComplaint,
        Notes = visit.Notes,
        CreatedAt = visit.CreatedAt,
        CreatedBy = visit.CreatedBy,
        UpdatedAt = visit.UpdatedAt,
        UpdatedBy = visit.UpdatedBy
    };

    private static PhysioSessionDto MapToPhysioSessionDto(PhysioSession session) => new()
    {
        Id = session.Id,
        PatientId = session.PatientId,
        PatientName = session.Patient?.FullNameEn,
        BranchId = session.BranchId,
        BranchName = session.Branch?.NameEn,
        ProviderName = session.PhysiotherapistId,
        VisitDate = session.SessionDate,
        Notes = session.Notes,
        DurationMinutes = session.DurationMinutes,
        PainLevelBefore = session.PainLevelBefore,
        PainLevelAfter = session.PainLevelAfter,
        TechniquesUsed = session.ModalitiesUsed,
        PatientResponse = session.PatientResponse,
        HomeExercises = session.HomeInstructions,
        SessionNumber = session.SessionNumber,
        CreatedAt = session.CreatedAt,
        CreatedBy = session.CreatedBy,
        UpdatedAt = session.UpdatedAt,
        UpdatedBy = session.UpdatedBy
    };

    private static ECGRecordDto MapToECGRecordDto(ECGRecord ecg) => new()
    {
        Id = ecg.Id,
        PatientId = ecg.PatientId,
        RecordDate = ecg.RecordDate,
        Rhythm = ecg.Rhythm.ToString(),
        HeartRate = ecg.HeartRate,
        PRInterval = ecg.PRInterval?.ToString(),
        QRSDuration = ecg.QRSDuration?.ToString(),
        QTInterval = ecg.QTInterval?.ToString(),
        Interpretation = ecg.Interpretation,
        PerformedBy = ecg.CreatedBy,
        FilePath = ecg.FilePath
    };

    private static EyePrescriptionDto MapToEyePrescriptionDto(EyePrescription prescription) => new()
    {
        Id = prescription.Id,
        PatientId = prescription.PatientId,
        PrescriptionDate = prescription.PrescriptionDate,
        RightSphere = prescription.SphereOd,
        RightCylinder = prescription.CylinderOd,
        RightAxis = prescription.AxisOd,
        RightAdd = prescription.AddOd,
        LeftSphere = prescription.SphereOs,
        LeftCylinder = prescription.CylinderOs,
        LeftAxis = prescription.AxisOs,
        LeftAdd = prescription.AddOs,
        PupillaryDistance = prescription.PupillaryDistance?.ToString(),
        LensType = prescription.LensType,
        Notes = prescription.Notes,
        ExpiryDate = prescription.ExpiryDate
    };

    private static PhysioAssessmentDto MapToPhysioAssessmentDto(PhysioAssessment assessment) => new()
    {
        Id = assessment.Id,
        PatientId = assessment.PatientId,
        AssessmentDate = assessment.AssessmentDate,
        Diagnosis = assessment.Diagnosis,
        PresentingComplaint = assessment.PresentingComplaint,
        HistoryOfPresentIllness = assessment.HistoryOfPresentIllness,
        PastMedicalHistory = assessment.PastMedicalHistory,
        ObjectiveFindings = assessment.ObjectiveFindings,
        RangeOfMotion = assessment.RangeOfMotion,
        StrengthAssessment = assessment.StrengthAssessment,
        FunctionalLimitations = assessment.FunctionalLimitations,
        TreatmentGoals = assessment.TreatmentGoals,
        TreatmentPlan = assessment.TreatmentPlan,
        PlannedSessions = assessment.PlannedSessions,
        AssessedBy = assessment.PhysiotherapistId
    };

    #endregion
}
