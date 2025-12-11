using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for patient consent management.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConsentController : BaseApiController
{
    private readonly IConsentService _consentService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<ConsentController> _logger;

    public ConsentController(
        IConsentService consentService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        ILogger<ConsentController> logger)
    {
        _consentService = consentService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Patient Consents

    /// <summary>
    /// Get consents for a patient.
    /// </summary>
    [HttpGet("patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientConsentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientConsents(
        int patientId,
        [FromQuery] string? consentType = null,
        [FromQuery] bool activeOnly = false)
    {
        var consents = await _consentService.GetPatientConsentsAsync(patientId, consentType, activeOnly);
        return ApiOk(consents);
    }

    /// <summary>
    /// Get consent summary for a patient.
    /// </summary>
    [HttpGet("patient/{patientId:int}/summary")]
    [ProducesResponseType(typeof(ApiResponse<PatientConsentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientConsentSummary(int patientId)
    {
        var summary = await _consentService.GetPatientConsentSummaryAsync(patientId);
        return ApiOk(summary);
    }

    /// <summary>
    /// Get consent by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatientConsentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConsent(int id)
    {
        var consent = await _consentService.GetConsentByIdAsync(id);
        if (consent == null)
        {
            return ApiNotFound("Consent not found");
        }
        return ApiOk(consent);
    }

    /// <summary>
    /// Grant consent.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PatientConsentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> GrantConsent([FromBody] SaveConsentDto dto)
    {
        var userId = _userContext.UserId ?? 0;
        var consent = await _consentService.GrantConsentAsync(dto, userId);

        _logger.LogInformation(
            "Consent granted: Patient {PatientId}, Type {ConsentType}, By User {UserId}",
            dto.PatientId, dto.ConsentType, userId);

        return ApiCreated(consent, $"/api/consent/{consent.Id}");
    }

    /// <summary>
    /// Revoke consent.
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(typeof(ApiResponse<PatientConsentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeConsent([FromBody] RevokeConsentDto dto)
    {
        var userId = _userContext.UserId ?? 0;
        var consent = await _consentService.RevokeConsentAsync(dto, userId);

        _logger.LogInformation(
            "Consent revoked: ConsentId {ConsentId}, Reason: {Reason}, By User {UserId}",
            dto.ConsentId, dto.Reason, userId);

        return ApiOk(consent);
    }

    /// <summary>
    /// Update consent.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PatientConsentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConsent(int id, [FromBody] SaveConsentDto dto)
    {
        var userId = _userContext.UserId ?? 0;
        var consent = await _consentService.UpdateConsentAsync(id, dto, userId);
        if (consent == null)
        {
            return ApiNotFound("Consent not found");
        }

        _logger.LogInformation(
            "Consent updated: ConsentId {ConsentId}, By User {UserId}",
            id, userId);

        return ApiOk(consent);
    }

    /// <summary>
    /// Get consent history.
    /// </summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<List<ConsentHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsentHistory(int id)
    {
        var history = await _consentService.GetConsentHistoryAsync(id);
        return ApiOk(history);
    }

    #endregion

    #region Consent Verification

    /// <summary>
    /// Verify patient has required consent.
    /// </summary>
    [HttpGet("verify/{patientId:int}/{consentType}")]
    [ProducesResponseType(typeof(ApiResponse<ConsentVerificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyConsent(int patientId, string consentType)
    {
        var verification = await _consentService.VerifyConsentAsync(patientId, consentType);
        return ApiOk(verification);
    }

    /// <summary>
    /// Verify patient has required consent for a specific purpose.
    /// </summary>
    [HttpGet("verify/{patientId:int}/{consentType}/{purpose}")]
    [ProducesResponseType(typeof(ApiResponse<ConsentVerificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyConsentForPurpose(int patientId, string consentType, string purpose)
    {
        var verification = await _consentService.VerifyConsentForPurposeAsync(patientId, consentType, purpose);
        return ApiOk(verification);
    }

    /// <summary>
    /// Check all required consents for a patient.
    /// </summary>
    [HttpPost("verify/bulk")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, ConsentVerificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyMultipleConsents([FromBody] BulkConsentVerificationRequest request)
    {
        var results = await _consentService.VerifyMultipleConsentsAsync(request.PatientId, request.ConsentTypes);
        return ApiOk(results);
    }

    #endregion

    #region Consent Form Templates

    /// <summary>
    /// Get consent form templates.
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<List<ConsentFormTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] string? consentType = null,
        [FromQuery] string? language = null)
    {
        var templates = await _consentService.GetConsentFormTemplatesAsync(consentType, language);
        return ApiOk(templates);
    }

    /// <summary>
    /// Get consent form template by ID.
    /// </summary>
    [HttpGet("templates/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ConsentFormTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(int id)
    {
        var template = await _consentService.GetConsentFormTemplateAsync(id);
        if (template == null)
        {
            return ApiNotFound("Template not found");
        }
        return ApiOk(template);
    }

    /// <summary>
    /// Create consent form template.
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<ConsentFormTemplateDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTemplate([FromBody] ConsentFormTemplateDto dto)
    {
        var template = await _consentService.CreateConsentFormTemplateAsync(dto);
        return ApiCreated(template, $"/api/consent/templates/{template.Id}");
    }

    /// <summary>
    /// Update consent form template.
    /// </summary>
    [HttpPut("templates/{id:int}")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<ConsentFormTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] ConsentFormTemplateDto dto)
    {
        dto.Id = id;
        var template = await _consentService.UpdateConsentFormTemplateAsync(dto);
        if (template == null)
        {
            return ApiNotFound("Template not found");
        }
        return ApiOk(template);
    }

    #endregion

    #region FHIR/HIE

    /// <summary>
    /// Export consent as FHIR R4 Consent resource.
    /// </summary>
    [HttpGet("{id:int}/fhir")]
    [ProducesResponseType(typeof(ApiResponse<ConsentDirectiveDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToFhir(int id)
    {
        var fhirConsent = await _consentService.ExportToFhirAsync(id);
        if (fhirConsent == null)
        {
            return ApiNotFound("Consent not found");
        }
        return ApiOk(fhirConsent);
    }

    /// <summary>
    /// Get consent directive for HIE exchange.
    /// </summary>
    [HttpGet("patient/{patientId:int}/directive")]
    [ProducesResponseType(typeof(ApiResponse<ConsentDirectiveDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsentDirective(int patientId)
    {
        var directive = await _consentService.GetConsentDirectiveAsync(patientId);
        return ApiOk(directive);
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get expiring consents.
    /// </summary>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientConsentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringConsents([FromQuery] int daysAhead = 30)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var consents = await _consentService.GetExpiringConsentsAsync(branchId, daysAhead);
        return ApiOk(consents);
    }

    /// <summary>
    /// Get consent compliance report.
    /// </summary>
    [HttpGet("compliance-report")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<ConsentComplianceReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComplianceReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var report = await _consentService.GetComplianceReportAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(report);
    }

    #endregion
}

/// <summary>
/// Bulk consent verification request
/// </summary>
public class BulkConsentVerificationRequest
{
    public int PatientId { get; set; }
    public List<string> ConsentTypes { get; set; } = new();
}

/// <summary>
/// Consent compliance report DTO
/// </summary>
public class ConsentComplianceReportDto
{
    public int BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalPatients { get; set; }
    public int PatientsWithAllConsents { get; set; }
    public int PatientsWithMissingConsents { get; set; }
    public int TotalConsentsGranted { get; set; }
    public int TotalConsentsRevoked { get; set; }
    public int TotalConsentsExpired { get; set; }
    public Dictionary<string, int> ConsentsByType { get; set; } = new();
    public List<ConsentComplianceIssueDto> ComplianceIssues { get; set; } = new();
}

/// <summary>
/// Consent compliance issue
/// </summary>
public class ConsentComplianceIssueDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
