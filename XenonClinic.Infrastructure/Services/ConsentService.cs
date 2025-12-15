using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Patient consent management service implementation
/// </summary>
public class ConsentService : IConsentService
{
    private readonly ClinicDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(ClinicDbContext context, IAuditService auditService, ILogger<ConsentService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    #region Consent CRUD

    public async Task<PatientConsentDto?> GetConsentByIdAsync(int consentId)
    {
        var consent = await _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Include(c => c.History)
            .FirstOrDefaultAsync(c => c.Id == consentId);

        if (consent == null)
            throw new InvalidOperationException("Consent not found");

        return MapToDto(consent);
    }

    public async Task<List<PatientConsentDto>> GetPatientConsentsAsync(int patientId, string? consentType = null, bool activeOnly = false)
    {
        var query = _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Where(c => c.PatientId == patientId);

        if (!string.IsNullOrEmpty(consentType))
            query = query.Where(c => c.ConsentType == consentType);

        if (activeOnly)
            query = query.Where(c => c.Status == ConsentStatuses.Active);

        var consents = await query
            .OrderByDescending(c => c.GrantedDate)
            .ToListAsync();

        return consents.Select(MapToDto).ToList();
    }

    public async Task<PatientConsentDto> GrantConsentAsync(SaveConsentDto request, int grantedByUserId)
    {
        var consent = new PatientConsent
        {
            PatientId = request.PatientId,
            ConsentType = request.ConsentType,
            ConsentCategory = request.ConsentCategory,
            Description = request.Description,
            Status = ConsentStatuses.Active,
            GrantedDate = DateTime.UtcNow,
            GrantedByUserId = grantedByUserId,
            ExpirationDate = request.ExpirationDate,
            DocumentPath = request.DocumentPath,
            SignatureData = request.SignatureData,
            WitnessName = request.WitnessName,
            ScopeJson = request.Scope != null ? JsonSerializer.Serialize(request.Scope) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<PatientConsent>().Add(consent);
        await _context.SaveChangesAsync();

        // Add history entry
        var history = new ConsentHistory
        {
            ConsentId = consent.Id,
            Action = "CREATED",
            NewStatus = ConsentStatuses.Active,
            ActionDate = DateTime.UtcNow,
            ActionByUserId = grantedByUserId
        };
        _context.Set<ConsentHistory>().Add(history);
        await _context.SaveChangesAsync();

        await _auditService.LogPHIAccessAsync(grantedByUserId, request.PatientId, "CONSENT", "CREATE", consent.Id.ToString());
        _logger.LogInformation("Created consent {ConsentId} for patient {PatientId}", consent.Id, request.PatientId);

        return await GetConsentByIdAsync(consent.Id);
    }

    public async Task<PatientConsentDto?> UpdateConsentAsync(int consentId, SaveConsentDto request, int updatedByUserId)
    {
        var consent = await _context.Set<PatientConsent>().FindAsync(consentId);
        if (consent == null)
            throw new InvalidOperationException("Consent not found");

        var oldStatus = consent.Status;
        consent.Description = request.Description;
        consent.ExpirationDate = request.ExpirationDate;
        consent.DocumentPath = request.DocumentPath;
        consent.SignatureData = request.SignatureData;
        consent.WitnessName = request.WitnessName;
        consent.ScopeJson = request.Scope != null ? JsonSerializer.Serialize(request.Scope) : null;
        consent.UpdatedAt = DateTime.UtcNow;

        var history = new ConsentHistory
        {
            ConsentId = consent.Id,
            Action = "UPDATED",
            PreviousStatus = oldStatus,
            NewStatus = consent.Status,
            ActionDate = DateTime.UtcNow,
            ActionByUserId = updatedByUserId
        };
        _context.Set<ConsentHistory>().Add(history);
        await _context.SaveChangesAsync();

        await _auditService.LogPHIAccessAsync(updatedByUserId, consent.PatientId, "CONSENT", "UPDATE", consentId.ToString());
        return await GetConsentByIdAsync(consentId);
    }

    public async Task<PatientConsentDto> RevokeConsentAsync(RevokeConsentDto request, int revokedByUserId)
    {
        var consent = await _context.Set<PatientConsent>().FindAsync(request.ConsentId);
        if (consent == null)
            throw new InvalidOperationException("Consent not found");

        var oldStatus = consent.Status;
        consent.Status = ConsentStatuses.Revoked;
        consent.RevokedDate = request.EffectiveDate ?? DateTime.UtcNow;
        consent.RevokedByUserId = revokedByUserId;
        consent.RevokedReason = request.Reason;

        var history = new ConsentHistory
        {
            ConsentId = consent.Id,
            Action = "REVOKED",
            PreviousStatus = oldStatus,
            NewStatus = ConsentStatuses.Revoked,
            ActionDate = DateTime.UtcNow,
            ActionByUserId = revokedByUserId,
            Reason = request.Reason
        };
        _context.Set<ConsentHistory>().Add(history);
        await _context.SaveChangesAsync();

        await _auditService.LogPHIAccessAsync(revokedByUserId, consent.PatientId, "CONSENT", "REVOKE", request.ConsentId.ToString(), request.Reason);
        _logger.LogInformation("Revoked consent {ConsentId} for patient {PatientId}: {Reason}", consent.Id, consent.PatientId, request.Reason);

        return await GetConsentByIdAsync(consent.Id);
    }

    public async Task<List<ConsentHistoryDto>> GetConsentHistoryAsync(int consentId)
    {
        var history = await _context.Set<ConsentHistory>()
            .Where(h => h.ConsentId == consentId)
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        return history.Select(h => new ConsentHistoryDto
        {
            Id = h.Id,
            Action = h.Action,
            PreviousStatus = h.PreviousStatus,
            NewStatus = h.NewStatus,
            ActionDate = h.ActionDate,
            ActionByUserId = h.ActionByUserId,
            Reason = h.Reason
        }).ToList();
    }

    #endregion

    #region Consent Verification

    public async Task<ConsentVerificationDto> VerifyConsentAsync(int patientId, string consentType)
    {
        var consent = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId && c.ConsentType == consentType && c.Status == ConsentStatuses.Active)
            .OrderByDescending(c => c.GrantedDate)
            .FirstOrDefaultAsync();

        if (consent == null)
        {
            return new ConsentVerificationDto
            {
                HasConsent = false,
                ConsentType = consentType,
                Status = "NOT_FOUND",
                DenialReason = $"No active {consentType} consent found for patient"
            };
        }

        var isExpired = consent.ExpirationDate.HasValue && consent.ExpirationDate.Value < DateTime.UtcNow;

        if (isExpired)
        {
            // Update status to expired
            consent.Status = ConsentStatuses.Expired;
            await _context.SaveChangesAsync();
        }

        var scope = consent.ScopeJson != null
            ? JsonSerializer.Deserialize<ConsentScopeDto>(consent.ScopeJson)
            : null;

        return new ConsentVerificationDto
        {
            HasConsent = !isExpired,
            ConsentType = consentType,
            Status = consent.Status,
            GrantedDate = consent.GrantedDate,
            ExpirationDate = consent.ExpirationDate,
            IsExpired = isExpired,
            AllowedPurposes = scope?.AllowedPurposes,
            DenialReason = isExpired ? "Consent has expired" : null
        };
    }

    public async Task<ConsentVerificationDto> VerifyConsentForPurposeAsync(int patientId, string consentType, string purpose)
    {
        var consent = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId && c.ConsentType == consentType && c.Status == ConsentStatuses.Active)
            .OrderByDescending(c => c.GrantedDate)
            .FirstOrDefaultAsync();

        if (consent == null)
        {
            return new ConsentVerificationDto
            {
                HasConsent = false,
                ConsentType = consentType,
                Status = "NOT_FOUND",
                DenialReason = $"No active {consentType} consent found for patient"
            };
        }

        var isExpired = consent.ExpirationDate.HasValue && consent.ExpirationDate.Value < DateTime.UtcNow;

        if (isExpired)
        {
            // Update status to expired
            consent.Status = ConsentStatuses.Expired;
            await _context.SaveChangesAsync();
        }

        var scope = consent.ScopeJson != null
            ? JsonSerializer.Deserialize<ConsentScopeDto>(consent.ScopeJson)
            : null;

        // Check purpose
        if (scope?.AllowedPurposes != null && !scope.AllowedPurposes.Contains(purpose))
        {
            return new ConsentVerificationDto
            {
                HasConsent = false,
                ConsentType = consentType,
                Status = consent.Status,
                GrantedDate = consent.GrantedDate,
                ExpirationDate = consent.ExpirationDate,
                IsExpired = isExpired,
                DenialReason = $"Purpose '{purpose}' not covered by consent"
            };
        }

        return new ConsentVerificationDto
        {
            HasConsent = !isExpired,
            ConsentType = consentType,
            Status = consent.Status,
            GrantedDate = consent.GrantedDate,
            ExpirationDate = consent.ExpirationDate,
            IsExpired = isExpired,
            AllowedPurposes = scope?.AllowedPurposes,
            DenialReason = isExpired ? "Consent has expired" : null
        };
    }

    public async Task<Dictionary<string, ConsentVerificationDto>> VerifyMultipleConsentsAsync(int patientId, List<string> consentTypes)
    {
        var results = new Dictionary<string, ConsentVerificationDto>();

        foreach (var consentType in consentTypes)
        {
            var verification = await VerifyConsentAsync(patientId, consentType);
            results[consentType] = verification;
        }

        return results;
    }

    public async Task<bool> HasActiveConsentAsync(int patientId, string consentType)
    {
        var result = await VerifyConsentAsync(patientId, consentType);
        return result.HasConsent;
    }

    public async Task<bool> CanShareDataAsync(int patientId, string recipientType, string purpose)
    {
        var consent = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId && c.ConsentType == ConsentTypes.DataSharing && c.Status == ConsentStatuses.Active)
            .FirstOrDefaultAsync();

        if (consent == null) return false;

        var scope = consent.ScopeJson != null 
            ? JsonSerializer.Deserialize<ConsentScopeDto>(consent.ScopeJson) 
            : null;

        if (scope == null) return false;

        var recipientAllowed = scope.AllowedRecipients?.Contains(recipientType) ?? false;
        var purposeAllowed = scope.AllowedPurposes?.Contains(purpose) ?? false;

        return recipientAllowed && purposeAllowed && scope.AllowDataSharing;
    }

    public async Task<bool> CanParticipateInResearchAsync(int patientId)
    {
        var consent = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId && c.ConsentType == ConsentTypes.ResearchParticipation && c.Status == ConsentStatuses.Active)
            .FirstOrDefaultAsync();

        if (consent == null) return false;

        var scope = consent.ScopeJson != null 
            ? JsonSerializer.Deserialize<ConsentScopeDto>(consent.ScopeJson) 
            : null;

        return scope?.AllowResearch ?? false;
    }

    #endregion

    #region Consent Summary

    public async Task<PatientConsentSummaryDto> GetPatientConsentSummaryAsync(int patientId)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        var consents = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId)
            .ToListAsync();

        // Check for expired consents and update status
        foreach (var consent in consents.Where(c => c.Status == ConsentStatuses.Active && c.ExpirationDate.HasValue && c.ExpirationDate.Value < DateTime.UtcNow))
        {
            consent.Status = ConsentStatuses.Expired;
        }
        await _context.SaveChangesAsync();

        return new PatientConsentSummaryDto
        {
            PatientId = patientId,
            PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
            TotalConsents = consents.Count,
            ActiveConsents = consents.Count(c => c.Status == ConsentStatuses.Active),
            RevokedConsents = consents.Count(c => c.Status == ConsentStatuses.Revoked),
            ExpiredConsents = consents.Count(c => c.Status == ConsentStatuses.Expired),
            PendingConsents = consents.Count(c => c.Status == ConsentStatuses.Pending),
            ConsentsByType = consents.GroupBy(c => c.ConsentType).Select(g => {
                var latest = g.OrderByDescending(c => c.GrantedDate).FirstOrDefault();
                return new ConsentStatusItemDto
                {
                    ConsentType = g.Key,
                    Status = latest?.Status ?? string.Empty,
                    GrantedDate = latest?.GrantedDate,
                    ExpirationDate = latest?.ExpirationDate,
                    NeedsRenewal = latest?.ExpirationDate.HasValue == true && latest.ExpirationDate.Value.AddDays(-30) < DateTime.UtcNow
                };
            }).ToList(),
            LastConsentDate = consents.Any() ? consents.Max(c => c.GrantedDate) : null,
            HasTreatmentConsent = consents.Any(c => c.ConsentType == ConsentTypes.TreatmentConsent && c.Status == ConsentStatuses.Active),
            HasHIPAAConsent = consents.Any(c => c.ConsentType == ConsentTypes.HIPAAConsent && c.Status == ConsentStatuses.Active),
            HasResearchConsent = consents.Any(c => c.ConsentType == ConsentTypes.ResearchParticipation && c.Status == ConsentStatuses.Active)
        };
    }

    public async Task<List<PatientConsentDto>> GetExpiringConsentsAsync(int branchId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        var consents = await _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Where(c => c.Patient.BranchId == branchId && c.Status == ConsentStatuses.Active && c.ExpirationDate != null && c.ExpirationDate <= cutoffDate)
            .OrderBy(c => c.ExpirationDate)
            .ToListAsync();

        return consents.Select(MapToDto).ToList();
    }

    public async Task<List<PatientConsentDto>> GetPendingConsentsAsync(int? branchId = null)
    {
        var query = _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Where(c => c.Status == ConsentStatuses.Pending);

        if (branchId.HasValue)
            query = query.Where(c => c.Patient.BranchId == branchId.Value);

        var consents = await query.OrderBy(c => c.CreatedAt).ToListAsync();
        return consents.Select(MapToDto).ToList();
    }

    public async Task<object> GetComplianceReportAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var consents = await _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Where(c => c.Patient.BranchId == branchId && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
            .ToListAsync();

        var totalConsents = consents.Count;
        var activeConsents = consents.Count(c => c.Status == ConsentStatuses.Active);
        var revokedConsents = consents.Count(c => c.Status == ConsentStatuses.Revoked);
        var expiredConsents = consents.Count(c => c.Status == ConsentStatuses.Expired);
        var pendingConsents = consents.Count(c => c.Status == ConsentStatuses.Pending);

        var consentsByType = consents.GroupBy(c => c.ConsentType)
            .Select(g => new
            {
                ConsentType = g.Key,
                Total = g.Count(),
                Active = g.Count(c => c.Status == ConsentStatuses.Active),
                Revoked = g.Count(c => c.Status == ConsentStatuses.Revoked),
                Expired = g.Count(c => c.Status == ConsentStatuses.Expired)
            })
            .ToList();

        return new
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate,
            TotalConsents = totalConsents,
            ActiveConsents = activeConsents,
            RevokedConsents = revokedConsents,
            ExpiredConsents = expiredConsents,
            PendingConsents = pendingConsents,
            ComplianceRate = totalConsents > 0 ? (double)activeConsents / totalConsents * 100 : 0,
            ConsentsByType = consentsByType,
            GeneratedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Templates

    public async Task<List<ConsentFormTemplateDto>> GetConsentFormTemplatesAsync(string? consentType = null, string? language = null)
    {
        var query = _context.Set<ConsentFormTemplate>().Where(t => t.IsActive);
        if (!string.IsNullOrEmpty(consentType))
            query = query.Where(t => t.ConsentType == consentType);
        if (!string.IsNullOrEmpty(language))
            query = query.Where(t => t.Language == language);

        var templates = await query.OrderBy(t => t.TemplateName).ToListAsync();
        return templates.Select(t => new ConsentFormTemplateDto
        {
            Id = t.Id,
            TemplateName = t.TemplateName,
            ConsentType = t.ConsentType,
            ConsentCategory = t.ConsentCategory,
            Description = t.Description,
            TemplateContent = t.TemplateContent,
            Language = t.Language,
            Version = t.Version,
            IsActive = t.IsActive,
            RequiresWitness = t.RequiresWitness,
            ValidityDays = t.ValidityDays
        }).ToList();
    }

    public async Task<ConsentFormTemplateDto?> GetConsentFormTemplateAsync(int templateId)
    {
        var t = await _context.Set<ConsentFormTemplate>().FindAsync(templateId);
        if (t == null) throw new InvalidOperationException("Template not found");

        return new ConsentFormTemplateDto
        {
            Id = t.Id,
            TemplateName = t.TemplateName,
            ConsentType = t.ConsentType,
            ConsentCategory = t.ConsentCategory,
            Description = t.Description,
            TemplateContent = t.TemplateContent,
            Language = t.Language,
            Version = t.Version,
            IsActive = t.IsActive,
            RequiresWitness = t.RequiresWitness,
            ValidityDays = t.ValidityDays
        };
    }

    public async Task<ConsentFormTemplateDto> CreateConsentFormTemplateAsync(ConsentFormTemplateDto template)
    {
        var entity = new ConsentFormTemplate
        {
            TemplateName = template.TemplateName,
            ConsentType = template.ConsentType,
            ConsentCategory = template.ConsentCategory,
            Description = template.Description,
            TemplateContent = template.TemplateContent,
            Language = template.Language,
            Version = template.Version,
            IsActive = template.IsActive,
            RequiresWitness = template.RequiresWitness,
            ValidityDays = template.ValidityDays,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ConsentFormTemplate>().Add(entity);
        await _context.SaveChangesAsync();

        template.Id = entity.Id;
        return template;
    }

    public async Task<ConsentFormTemplateDto?> UpdateConsentFormTemplateAsync(ConsentFormTemplateDto template)
    {
        var entity = await _context.Set<ConsentFormTemplate>().FindAsync(template.Id);
        if (entity == null)
            return null;

        entity.TemplateName = template.TemplateName;
        entity.ConsentType = template.ConsentType;
        entity.ConsentCategory = template.ConsentCategory;
        entity.Description = template.Description;
        entity.TemplateContent = template.TemplateContent;
        entity.Language = template.Language;
        entity.Version = template.Version;
        entity.IsActive = template.IsActive;
        entity.RequiresWitness = template.RequiresWitness;
        entity.ValidityDays = template.ValidityDays;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<string> GenerateConsentDocumentAsync(int templateId, int patientId)
    {
        var template = await GetConsentFormTemplateAsync(templateId);
        var patient = await _context.Patients.FindAsync(patientId);
        
        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        // Replace placeholders in template
        var document = template.TemplateContent
            .Replace("{{PatientName}}", $"{patient.FirstName} {patient.LastName}")
            .Replace("{{PatientDOB}}", patient.DateOfBirth?.ToString("MM/dd/yyyy") ?? "")
            .Replace("{{Date}}", DateTime.UtcNow.ToString("MM/dd/yyyy"))
            .Replace("{{ConsentType}}", template.ConsentType);

        return document;
    }

    #endregion

    #region HIE/FHIR Integration

    public async Task<ConsentDirectiveDto> GetConsentDirectiveAsync(int patientId)
    {
        var consents = await _context.Set<PatientConsent>()
            .Where(c => c.PatientId == patientId && c.Status == ConsentStatuses.Active)
            .ToListAsync();

        var dataSharingConsent = consents.FirstOrDefault(c => c.ConsentType == ConsentTypes.DataSharing || c.ConsentType == ConsentTypes.HIEParticipation);
        
        if (dataSharingConsent == null)
        {
            return new ConsentDirectiveDto
            {
                PatientId = patientId,
                DirectiveType = "deny",
                Status = "active",
                Actions = new List<string> { "access", "collect", "use", "disclose" }
            };
        }

        var scope = dataSharingConsent.ScopeJson != null 
            ? JsonSerializer.Deserialize<ConsentScopeDto>(dataSharingConsent.ScopeJson) 
            : null;

        return new ConsentDirectiveDto
        {
            PatientId = patientId,
            DirectiveType = scope?.AllowDataSharing == true ? "permit" : "deny",
            Status = "active",
            StartDate = dataSharingConsent.GrantedDate,
            EndDate = dataSharingConsent.ExpirationDate,
            Actors = scope?.AllowedRecipients,
            Actions = new List<string> { "access", "collect" },
            Purposes = scope?.AllowedPurposes,
            DataCategories = scope?.DataCategories
        };
    }

    public async Task<ConsentDirectiveDto?> ExportToFhirAsync(int consentId)
    {
        var consent = await GetConsentByIdAsync(consentId);
        if (consent == null) return null;

        return new ConsentDirectiveDto
        {
            PatientId = consent.PatientId,
            DirectiveType = consent.Scope?.AllowDataSharing == true ? "permit" : "deny",
            Status = consent.Status == ConsentStatuses.Active ? "active" : consent.Status == ConsentStatuses.Revoked ? "rejected" : "inactive",
            StartDate = consent.GrantedDate,
            EndDate = consent.ExpirationDate,
            Actors = consent.Scope?.AllowedRecipients,
            Actions = new List<string> { "access", "collect" },
            Purposes = consent.Scope?.AllowedPurposes,
            DataCategories = consent.Scope?.DataCategories
        };
    }

    public async Task<PatientConsentDto> ImportFromFhirConsentAsync(object fhirConsent, int patientId)
    {
        // Parse FHIR Consent resource and create local consent
        var json = JsonSerializer.Serialize(fhirConsent);
        var fhir = JsonSerializer.Deserialize<JsonElement>(json);

        var request = new SaveConsentDto
        {
            PatientId = patientId,
            ConsentType = ConsentTypes.DataSharing,
            ConsentCategory = ConsentCategories.Privacy,
            Description = "Imported from FHIR",
            Scope = new ConsentScopeDto { AllowDataSharing = true }
        };

        return await GrantConsentAsync(request, 0);
    }

    #endregion

    #region Notifications

    public async Task SendConsentReminderAsync(int consentId)
    {
        var consent = await GetConsentByIdAsync(consentId);
        _logger.LogInformation("Sending consent renewal reminder for consent {ConsentId} to patient {PatientId}", consentId, consent.PatientId);
        // Integration with notification service would go here
    }

    public async Task ProcessExpiringConsentsAsync()
    {
        // Get all expiring consents across all branches
        var cutoffDate = DateTime.UtcNow.AddDays(30);
        var expiringConsents = await _context.Set<PatientConsent>()
            .Include(c => c.Patient)
            .Where(c => c.Status == ConsentStatuses.Active && c.ExpirationDate != null && c.ExpirationDate <= cutoffDate)
            .OrderBy(c => c.ExpirationDate)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var consentId in expiringConsents)
        {
            await SendConsentReminderAsync(consentId);
        }
        _logger.LogInformation("Processed {Count} expiring consents", expiringConsents.Count);
    }

    #endregion

    #region Mapping

    private static PatientConsentDto MapToDto(PatientConsent c) => new()
    {
        Id = c.Id,
        PatientId = c.PatientId,
        PatientName = c.Patient != null ? $"{c.Patient.FirstName} {c.Patient.LastName}" : "",
        ConsentType = c.ConsentType,
        ConsentCategory = c.ConsentCategory,
        Description = c.Description,
        Status = c.Status,
        GrantedDate = c.GrantedDate,
        RevokedDate = c.RevokedDate,
        ExpirationDate = c.ExpirationDate,
        RevokedReason = c.RevokedReason,
        DocumentPath = c.DocumentPath,
        SignatureData = c.SignatureData,
        WitnessName = c.WitnessName,
        Scope = c.ScopeJson != null ? JsonSerializer.Deserialize<ConsentScopeDto>(c.ScopeJson) : null,
        History = c.History?.Select(h => new ConsentHistoryDto
        {
            Id = h.Id, Action = h.Action, PreviousStatus = h.PreviousStatus, NewStatus = h.NewStatus,
            ActionDate = h.ActionDate, ActionByUserId = h.ActionByUserId, Reason = h.Reason
        }).ToList()
    };

    #endregion
}

#region Consent Entities

[Table("PatientConsents")]
public class PatientConsent
{
    [Key] public int Id { get; set; }
    public int PatientId { get; set; }
    [Required, MaxLength(50)] public string ConsentType { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string ConsentCategory { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    [Required, MaxLength(20)] public string Status { get; set; } = ConsentStatuses.Pending;
    public DateTime? GrantedDate { get; set; }
    public int? GrantedByUserId { get; set; }
    public DateTime? RevokedDate { get; set; }
    public int? RevokedByUserId { get; set; }
    [MaxLength(1000)] public string? RevokedReason { get; set; }
    public DateTime? ExpirationDate { get; set; }
    [MaxLength(500)] public string? DocumentPath { get; set; }
    public string? SignatureData { get; set; }
    [MaxLength(200)] public string? WitnessName { get; set; }
    public string? ScopeJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [ForeignKey(nameof(PatientId))] public virtual Core.Entities.Patient Patient { get; set; } = null!;
    public virtual ICollection<ConsentHistory> History { get; set; } = new List<ConsentHistory>();
}

[Table("ConsentHistory")]
public class ConsentHistory
{
    [Key] public int Id { get; set; }
    public int ConsentId { get; set; }
    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [MaxLength(20)] public string? PreviousStatus { get; set; }
    [Required, MaxLength(20)] public string NewStatus { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public int? ActionByUserId { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    [MaxLength(50)] public string? IpAddress { get; set; }
    [ForeignKey(nameof(ConsentId))] public virtual PatientConsent Consent { get; set; } = null!;
}

[Table("ConsentFormTemplates")]
public class ConsentFormTemplate
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string TemplateName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string ConsentType { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string ConsentCategory { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [Required] public string TemplateContent { get; set; } = string.Empty;
    [MaxLength(10)] public string Language { get; set; } = "en";
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public bool RequiresWitness { get; set; }
    public int? ValidityDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

#endregion
