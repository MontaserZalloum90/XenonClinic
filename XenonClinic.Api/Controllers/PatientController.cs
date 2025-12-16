using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for patient management operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientController : BaseApiController
{
    private readonly IPatientService _patientService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly IValidator<CreatePatientDto> _createValidator;
    private readonly IValidator<UpdatePatientDto> _updateValidator;
    private readonly IValidator<UploadPatientDocumentDto> _documentValidator;
    private readonly IValidator<PatientListRequestDto> _listValidator;
    private readonly ILogger<PatientController> _logger;

    public PatientController(
        IPatientService patientService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        IValidator<CreatePatientDto> createValidator,
        IValidator<UpdatePatientDto> updateValidator,
        IValidator<UploadPatientDocumentDto> documentValidator,
        IValidator<PatientListRequestDto> listValidator,
        ILogger<PatientController> logger)
    {
        _patientService = patientService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _documentValidator = documentValidator;
        _listValidator = listValidator;
        _logger = logger;
    }

    #region Patient CRUD

    /// <summary>
    /// Gets a paginated list of patients for the current branch.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<PatientSearchResultDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPatients([FromQuery] PatientListRequestDto request)
    {
        var validationResult = await _listValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        IEnumerable<Patient> patients;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            patients = await _patientService.SearchPatientsAsync(branchId.Value, request.SearchTerm);
        }
        else
        {
            patients = await _patientService.GetPatientsByBranchIdAsync(branchId.Value);
        }

        // Apply additional filters
        var query = patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            query = query.Where(p => p.Gender == request.Gender);
        }

        if (request.DateOfBirthFrom.HasValue)
        {
            query = query.Where(p => p.DateOfBirth >= request.DateOfBirthFrom.Value);
        }

        if (request.DateOfBirthTo.HasValue)
        {
            query = query.Where(p => p.DateOfBirth <= request.DateOfBirthTo.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortDescending);

        // Apply pagination
        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToSearchResult)
            .ToList();

        var paginatedResult = new PaginatedResponse<PatientSearchResultDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return ApiOk(paginatedResult);
    }

    /// <summary>
    /// Gets a patient by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);

        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        // Verify branch access
        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        var dto = MapToDto(patient);
        return ApiOk(dto);
    }

    /// <summary>
    /// Gets a patient by Emirates ID.
    /// </summary>
    [HttpGet("by-emirates-id/{emiratesId}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientByEmiratesId(string emiratesId)
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var patient = await _patientService.GetPatientByEmiratesIdAsync(emiratesId, branchId.Value);

        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        var dto = MapToDto(patient);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        // Check for duplicate Emirates ID
        var existingPatient = await _patientService.GetPatientByEmiratesIdAsync(dto.EmiratesId, branchId.Value);
        if (existingPatient != null)
        {
            return ApiConflict(PatientValidationMessages.EmiratesIdDuplicate);
        }

        var patient = new Patient
        {
            BranchId = branchId.Value,
            EmiratesId = dto.EmiratesId,
            FullNameEn = dto.FullNameEn,
            FullNameAr = dto.FullNameAr,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            HearingLossType = dto.HearingLossType,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _userContext.UserId
        };

        var createdPatient = await _patientService.CreatePatientAsync(patient);

        // Create medical history if provided
        if (dto.MedicalHistory != null)
        {
            var medicalHistory = new PatientMedicalHistory
            {
                PatientId = createdPatient.Id,
                ChronicConditions = dto.MedicalHistory.ChronicConditions,
                Allergies = dto.MedicalHistory.Allergies,
                CurrentMedications = dto.MedicalHistory.CurrentMedications,
                PastSurgeries = dto.MedicalHistory.PastSurgeries,
                FamilyHistory = dto.MedicalHistory.FamilyHistory,
                SocialHistory = dto.MedicalHistory.SocialHistory,
                OccupationalExposure = dto.MedicalHistory.OccupationalExposure,
                NoiseExposure = dto.MedicalHistory.NoiseExposure,
                Tinnitus = dto.MedicalHistory.Tinnitus,
                BalanceProblems = dto.MedicalHistory.BalanceProblems,
                IsSmoker = dto.MedicalHistory.IsSmoker,
                AlcoholConsumption = dto.MedicalHistory.AlcoholConsumption,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _userContext.UserId
            };

            await _patientService.CreateOrUpdateMedicalHistoryAsync(medicalHistory);
        }

        // SECURITY FIX: Do not log Emirates ID (PII) - log only last 4 characters for reference
        var maskedEmiratesId = createdPatient.EmiratesId.Length > 4
            ? $"***{createdPatient.EmiratesId[^4..]}"
            : "****";
        _logger.LogInformation(
            "Patient created: {PatientId}, EmiratesId: {MaskedEmiratesId}, Branch: {BranchId}, By: {UserId}",
            createdPatient.Id, maskedEmiratesId, branchId, _userContext.UserId);

        var resultDto = MapToDto(createdPatient);
        return ApiCreated(resultDto, $"/api/patient/{createdPatient.Id}");
    }

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
    {
        if (id != dto.Id)
        {
            return ApiBadRequest("Route ID does not match body ID");
        }

        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var existingPatient = await _patientService.GetPatientByIdAsync(id);
        if (existingPatient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        // Verify branch access
        if (!HasBranchAccess(existingPatient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        // Check for duplicate Emirates ID if changed
        if (existingPatient.EmiratesId != dto.EmiratesId)
        {
            var duplicatePatient = await _patientService.GetPatientByEmiratesIdAsync(
                dto.EmiratesId, existingPatient.BranchId);

            if (duplicatePatient != null && duplicatePatient.Id != id)
            {
                return ApiConflict(PatientValidationMessages.EmiratesIdDuplicate);
            }
        }

        // Update patient fields
        existingPatient.EmiratesId = dto.EmiratesId;
        existingPatient.FullNameEn = dto.FullNameEn;
        existingPatient.FullNameAr = dto.FullNameAr;
        existingPatient.DateOfBirth = dto.DateOfBirth;
        existingPatient.Gender = dto.Gender;
        existingPatient.PhoneNumber = dto.PhoneNumber;
        existingPatient.Email = dto.Email;
        existingPatient.HearingLossType = dto.HearingLossType;
        existingPatient.Notes = dto.Notes;
        existingPatient.UpdatedAt = DateTime.UtcNow;
        existingPatient.UpdatedBy = _userContext.UserId;

        await _patientService.UpdatePatientAsync(existingPatient);

        _logger.LogInformation(
            "Patient updated: {PatientId}, By: {UserId}",
            id, _userContext.UserId);

        var resultDto = MapToDto(existingPatient);
        return ApiOk(resultDto);
    }

    /// <summary>
    /// Deletes a patient (soft delete).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        // Verify branch access
        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        await _patientService.DeletePatientAsync(id);

        _logger.LogInformation(
            "Patient deleted (soft): {PatientId}, By: {UserId}",
            id, _userContext.UserId);

        return ApiOk("Patient deleted successfully");
    }

    #endregion

    #region Medical History

    /// <summary>
    /// Gets medical history for a patient.
    /// </summary>
    [HttpGet("{patientId:int}/medical-history")]
    [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicalHistory(int patientId)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        var medicalHistory = await _patientService.GetPatientMedicalHistoryAsync(patientId);
        if (medicalHistory == null)
        {
            return ApiNotFound("Medical history not found for this patient");
        }

        var dto = MapToMedicalHistoryDto(medicalHistory);
        return ApiOk(dto);
    }

    /// <summary>
    /// Creates or updates medical history for a patient.
    /// </summary>
    [HttpPut("{patientId:int}/medical-history")]
    [ProducesResponseType(typeof(ApiResponse<PatientMedicalHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveMedicalHistory(
        int patientId,
        [FromBody] CreatePatientMedicalHistoryDto dto)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        var medicalHistory = new PatientMedicalHistory
        {
            PatientId = patientId,
            ChronicConditions = dto.ChronicConditions,
            Allergies = dto.Allergies,
            CurrentMedications = dto.CurrentMedications,
            PastSurgeries = dto.PastSurgeries,
            FamilyHistory = dto.FamilyHistory,
            SocialHistory = dto.SocialHistory,
            OccupationalExposure = dto.OccupationalExposure,
            NoiseExposure = dto.NoiseExposure,
            Tinnitus = dto.Tinnitus,
            BalanceProblems = dto.BalanceProblems,
            IsSmoker = dto.IsSmoker,
            AlcoholConsumption = dto.AlcoholConsumption,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = _userContext.UserId
        };

        var saved = await _patientService.CreateOrUpdateMedicalHistoryAsync(medicalHistory);

        _logger.LogInformation(
            "Medical history saved for patient: {PatientId}, By: {UserId}",
            patientId, _userContext.UserId);

        var resultDto = MapToMedicalHistoryDto(saved);
        return ApiOk(resultDto);
    }

    #endregion

    #region Documents

    /// <summary>
    /// Gets all documents for a patient.
    /// </summary>
    [HttpGet("{patientId:int}/documents")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PatientDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientDocuments(int patientId)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        var documents = await _patientService.GetPatientDocumentsAsync(patientId);
        var dtos = documents.Select(MapToDocumentDto);

        return ApiOk(dtos);
    }

    /// <summary>
    /// Uploads a new document for a patient.
    /// </summary>
    [HttpPost("{patientId:int}/documents")]
    [ProducesResponseType(typeof(ApiResponse<PatientDocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        int patientId,
        [FromBody] UploadPatientDocumentDto dto)
    {
        dto.PatientId = patientId;

        var validationResult = await _documentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return ApiBadRequest("Validation failed", validationResult.Errors.ToDictionary(
                e => e.PropertyName,
                e => new[] { e.ErrorMessage }));
        }

        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        // BUG FIX: Validate file extension to prevent malicious file uploads
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".rtf" };
        var fileExtension = Path.GetExtension(dto.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            return ApiBadRequest($"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // BUG FIX: Validate content type matches extension
        var validContentTypes = new Dictionary<string, string[]>
        {
            { ".pdf", new[] { "application/pdf" } },
            { ".doc", new[] { "application/msword" } },
            { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { ".jpg", new[] { "image/jpeg" } },
            { ".jpeg", new[] { "image/jpeg" } },
            { ".png", new[] { "image/png" } },
            { ".gif", new[] { "image/gif" } },
            { ".txt", new[] { "text/plain" } },
            { ".rtf", new[] { "application/rtf", "text/rtf" } }
        };

        if (validContentTypes.TryGetValue(fileExtension, out var expectedContentTypes) &&
            !expectedContentTypes.Contains(dto.ContentType?.ToLowerInvariant()))
        {
            return ApiBadRequest("Content type does not match file extension");
        }

        // Decode file content with error handling
        byte[]? fileBytes = null;
        if (!string.IsNullOrEmpty(dto.FileContent))
        {
            try
            {
                fileBytes = Convert.FromBase64String(dto.FileContent);
            }
            catch (FormatException)
            {
                return ApiBadRequest("Invalid file content encoding. Expected valid base64 string.");
            }

            // BUG FIX: Validate file size (max 10MB)
            const int maxFileSizeBytes = 10 * 1024 * 1024;
            if (fileBytes.Length > maxFileSizeBytes)
            {
                return ApiBadRequest("File size exceeds maximum allowed (10MB)");
            }
        }

        var document = new PatientDocument
        {
            PatientId = patientId,
            DocumentName = dto.DocumentName,
            DocumentType = dto.DocumentType,
            Description = dto.Description,
            FileName = dto.FileName,
            FileExtension = fileExtension,
            FileSize = fileBytes?.Length ?? 0,
            ContentType = dto.ContentType,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = _userContext.UserId,
            ExpiryDate = dto.ExpiryDate,
            IsActive = true,
            Tags = dto.Tags
        };

        var uploaded = await _patientService.UploadDocumentAsync(document);

        _logger.LogInformation(
            "Document uploaded for patient: {PatientId}, DocumentId: {DocumentId}, By: {UserId}",
            patientId, uploaded.Id, _userContext.UserId);

        var resultDto = MapToDocumentDto(uploaded);
        return ApiCreated(resultDto, $"/api/patient/{patientId}/documents/{uploaded.Id}");
    }

    /// <summary>
    /// Deletes a patient document.
    /// </summary>
    [HttpDelete("{patientId:int}/documents/{documentId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(int patientId, int documentId)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null)
        {
            return ApiNotFound(PatientValidationMessages.PatientNotFound);
        }

        if (!HasBranchAccess(patient.BranchId))
        {
            throw new ForbiddenException(PatientValidationMessages.BranchAccessDenied);
        }

        var document = await _patientService.GetDocumentByIdAsync(documentId);
        if (document == null || document.PatientId != patientId)
        {
            return ApiNotFound(PatientValidationMessages.DocumentNotFound);
        }

        await _patientService.DeleteDocumentAsync(documentId);

        _logger.LogInformation(
            "Document deleted: {DocumentId}, Patient: {PatientId}, By: {UserId}",
            documentId, patientId, _userContext.UserId);

        return ApiOk("Document deleted successfully");
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets patient statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<PatientStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var branchId = GetCurrentBranchId();
        if (branchId == null)
        {
            return ApiBadRequest("Branch context is required");
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

        var statistics = new PatientStatisticsDto
        {
            TotalPatients = await _patientService.GetTotalPatientsCountAsync(branchId.Value),
            NewPatientsThisMonth = await _patientService.GetNewPatientsCountAsync(
                branchId.Value, startOfMonth, now),
            NewPatientsThisWeek = await _patientService.GetNewPatientsCountAsync(
                branchId.Value, startOfWeek, now),
            GenderDistribution = await _patientService.GetPatientsByGenderDistributionAsync(branchId.Value)
        };

        return ApiOk(statistics);
    }

    #endregion

    #region Helper Methods

    private int? GetCurrentBranchId()
    {
        return _tenantContext.BranchId;
    }

    private bool HasBranchAccess(int branchId)
    {
        if (_tenantContext.IsCompanyAdmin)
        {
            return true;
        }

        return _tenantContext.HasBranchAccess(branchId);
    }

    private static IQueryable<Patient> ApplySorting(
        IQueryable<Patient> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "fullnamear" => descending
                ? query.OrderByDescending(p => p.FullNameAr)
                : query.OrderBy(p => p.FullNameAr),
            "emiratesid" => descending
                ? query.OrderByDescending(p => p.EmiratesId)
                : query.OrderBy(p => p.EmiratesId),
            "dateofbirth" => descending
                ? query.OrderByDescending(p => p.DateOfBirth)
                : query.OrderBy(p => p.DateOfBirth),
            "createdat" => descending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            "gender" => descending
                ? query.OrderByDescending(p => p.Gender)
                : query.OrderBy(p => p.Gender),
            _ => descending
                ? query.OrderByDescending(p => p.FullNameEn)
                : query.OrderBy(p => p.FullNameEn)
        };
    }

    private static PatientDto MapToDto(Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            BranchId = patient.BranchId,
            BranchName = patient.Branch?.Name,
            EmiratesId = patient.EmiratesId,
            FullNameEn = patient.FullNameEn,
            FullNameAr = patient.FullNameAr,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            HearingLossType = patient.HearingLossType,
            Notes = patient.Notes,
            CreatedAt = patient.CreatedAt,
            CreatedBy = patient.CreatedBy,
            UpdatedAt = patient.UpdatedAt,
            UpdatedBy = patient.UpdatedBy,
            AppointmentsCount = patient.Appointments?.Count ?? 0,
            DocumentsCount = patient.Documents?.Count ?? 0,
            HasMedicalHistory = patient.MedicalHistory != null
        };
    }

    private static PatientSearchResultDto MapToSearchResult(Patient patient)
    {
        return new PatientSearchResultDto
        {
            Id = patient.Id,
            EmiratesId = patient.EmiratesId,
            FullNameEn = patient.FullNameEn,
            FullNameAr = patient.FullNameAr,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            PhoneNumber = patient.PhoneNumber,
            CreatedAt = patient.CreatedAt
        };
    }

    private static PatientMedicalHistoryDto MapToMedicalHistoryDto(PatientMedicalHistory history)
    {
        return new PatientMedicalHistoryDto
        {
            Id = history.Id,
            PatientId = history.PatientId,
            ChronicConditions = history.ChronicConditions,
            Allergies = history.Allergies,
            CurrentMedications = history.CurrentMedications,
            PastSurgeries = history.PastSurgeries,
            FamilyHistory = history.FamilyHistory,
            SocialHistory = history.SocialHistory,
            OccupationalExposure = history.OccupationalExposure,
            NoiseExposure = history.NoiseExposure,
            Tinnitus = history.Tinnitus,
            BalanceProblems = history.BalanceProblems,
            IsSmoker = history.IsSmoker,
            AlcoholConsumption = history.AlcoholConsumption,
            CreatedAt = history.CreatedAt,
            CreatedBy = history.CreatedBy,
            UpdatedAt = history.UpdatedAt,
            UpdatedBy = history.UpdatedBy
        };
    }

    private static PatientDocumentDto MapToDocumentDto(PatientDocument document)
    {
        return new PatientDocumentDto
        {
            Id = document.Id,
            PatientId = document.PatientId,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            Description = document.Description,
            FileName = document.FileName,
            FileExtension = document.FileExtension,
            FileSize = document.FileSize,
            ContentType = document.ContentType,
            UploadedAt = document.UploadedAt,
            UploadedBy = document.UploadedBy,
            ExpiryDate = document.ExpiryDate,
            IsActive = document.IsActive,
            Tags = document.Tags
        };
    }

    #endregion
}
