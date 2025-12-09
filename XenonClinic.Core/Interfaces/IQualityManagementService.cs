using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Quality management system service.
/// </summary>
public interface IQualityManagementService
{
    // Incidents
    Task<QualityIncident> ReportIncidentAsync(ReportIncidentRequest request);
    Task<QualityIncident?> GetIncidentAsync(int incidentId);
    Task<QualityIncident?> GetIncidentByNumberAsync(string incidentNumber);
    Task<IEnumerable<QualityIncident>> GetIncidentsAsync(IncidentSearchCriteria criteria);
    Task<QualityIncident> UpdateIncidentAsync(int incidentId, UpdateIncidentRequest request);
    Task UpdateIncidentStatusAsync(int incidentId, IncidentStatus status);
    Task AssignIncidentAsync(int incidentId, int assigneeEmployeeId);
    Task CloseIncidentAsync(int incidentId, string closureNotes);
    Task AddIncidentNoteAsync(int incidentId, string content, string noteType);

    // CAPA (Corrective & Preventive Actions)
    Task<CorrectiveAction> CreateCAPAAsync(CreateCAPARequest request);
    Task<CorrectiveAction?> GetCAPAAsync(int capaId);
    Task<IEnumerable<CorrectiveAction>> GetCAPAsAsync(CAPASearchCriteria criteria);
    Task<CorrectiveAction> UpdateCAPAAsync(int capaId, UpdateCAPARequest request);
    Task UpdateCAPAStatusAsync(int capaId, CAPAStatus status);
    Task AddCAPATaskAsync(int capaId, AddCAPATaskRequest request);
    Task CompleteCAPATaskAsync(int taskId, string? notes);
    Task VerifyCAPAEffectivenessAsync(int capaId, bool isEffective, string review);

    // Audits
    Task<QualityAudit> ScheduleAuditAsync(ScheduleAuditRequest request);
    Task<QualityAudit?> GetAuditAsync(int auditId);
    Task<IEnumerable<QualityAudit>> GetAuditsAsync(AuditSearchCriteria criteria);
    Task<QualityAudit> UpdateAuditAsync(int auditId, UpdateAuditRequest request);
    Task StartAuditAsync(int auditId);
    Task CompleteAuditAsync(int auditId, CompleteAuditRequest request);
    Task AddAuditFindingAsync(int auditId, AddAuditFindingRequest request);
    Task AddChecklistResultAsync(int auditId, int checklistItemId, ChecklistResult result, string? notes);

    // Metrics
    Task<QualityMetric> CreateMetricAsync(CreateMetricRequest request);
    Task<QualityMetric?> GetMetricAsync(int metricId);
    Task<IEnumerable<QualityMetric>> GetMetricsAsync(string? category = null);
    Task RecordMetricValueAsync(int metricId, RecordMetricValueRequest request);
    Task<IEnumerable<QualityMetricValue>> GetMetricValuesAsync(int metricId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<MetricTrend> GetMetricTrendAsync(int metricId, int periods = 12);

    // Compliance
    Task<ComplianceChecklist> CreateChecklistAsync(CreateChecklistRequest request);
    Task<IEnumerable<ComplianceChecklist>> GetChecklistsAsync(string? standard = null);
    Task AddChecklistItemAsync(int checklistId, AddChecklistItemRequest request);
    Task<ComplianceStatus> GetComplianceStatusAsync(string standard);

    // Dashboard & Reporting
    Task<QualityDashboard> GetDashboardAsync();
    Task<IEnumerable<IncidentTrend>> GetIncidentTrendsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<CAPAAnalysis>> GetCAPAAnalysisAsync(DateTime startDate, DateTime endDate);
}

// Request DTOs
public record ReportIncidentRequest(
    string Title,
    string Description,
    IncidentCategory Category,
    IncidentSeverity Severity,
    DateTime IncidentDate,
    int? PatientId,
    int? EmployeeId,
    int? DepartmentId,
    string? Location,
    string? ImmediateActions
);

public record UpdateIncidentRequest(
    string? Title,
    string? Description,
    IncidentSeverity? Severity,
    string? RootCause,
    string? ContributingFactors,
    DateTime? DueDate
);

public record IncidentSearchCriteria(
    IncidentCategory? Category = null,
    IncidentSeverity? Severity = null,
    IncidentStatus? Status = null,
    int? DepartmentId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

public record CreateCAPARequest(
    int? IncidentId,
    int? AuditFindingId,
    CAPAType Type,
    CAPAPriority Priority,
    string Title,
    string Description,
    string? RootCause,
    string ProposedAction,
    int? AssignedToEmployeeId,
    DateTime DueDate,
    int? DepartmentId
);

public record UpdateCAPARequest(
    string? Description,
    string? RootCause,
    string? ProposedAction,
    string? ActionTaken,
    DateTime? DueDate
);

public record CAPASearchCriteria(
    CAPAType? Type = null,
    CAPAStatus? Status = null,
    CAPAPriority? Priority = null,
    int? DepartmentId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
);

public record AddCAPATaskRequest(
    string Description,
    int? AssignedToEmployeeId,
    DateTime? DueDate
);

public record ScheduleAuditRequest(
    string Title,
    AuditType Type,
    DateTime ScheduledDate,
    string? Scope,
    string? Objectives,
    int? DepartmentId,
    int? LeadAuditorEmployeeId
);

public record AuditSearchCriteria(
    AuditType? Type = null,
    AuditStatus? Status = null,
    int? DepartmentId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
);

public record UpdateAuditRequest(
    string? Scope,
    string? Objectives,
    DateTime? ScheduledDate,
    int? LeadAuditorEmployeeId
);

public record CompleteAuditRequest(
    string Findings,
    string? Observations,
    string? Recommendations,
    string Conclusion
);

public record AddAuditFindingRequest(
    FindingType Type,
    FindingSeverity Severity,
    string Description,
    string? Evidence,
    string? CriteriaReference
);

public record CreateMetricRequest(
    string Code,
    string Name,
    string? Description,
    string? Category,
    MetricType Type,
    string? Formula,
    string? Unit,
    decimal? TargetValue,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    TrendDirection DesiredTrend,
    string? Frequency
);

public record RecordMetricValueRequest(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Value,
    decimal? Numerator,
    decimal? Denominator,
    string? Notes
);

public record CreateChecklistRequest(
    string Code,
    string Name,
    string? Description,
    string Standard,
    string? Version
);

public record AddChecklistItemRequest(
    int Sequence,
    string Category,
    string Requirement,
    string? Guidance,
    string? EvidenceRequired,
    bool IsMandatory = true
);

// Response DTOs
public record MetricTrend(
    int MetricId,
    string MetricName,
    decimal? TargetValue,
    TrendDirection DesiredTrend,
    List<MetricDataPoint> DataPoints,
    decimal? AverageValue,
    decimal? TrendPercentage,
    bool IsOnTarget
);

public record MetricDataPoint(
    DateTime PeriodStart,
    decimal Value,
    MetricStatus Status
);

public record ComplianceStatus(
    string Standard,
    int TotalRequirements,
    int CompliantCount,
    int NonCompliantCount,
    int PartialCount,
    int NotAssessedCount,
    decimal CompliancePercentage,
    List<string> CriticalGaps
);

public record QualityDashboard(
    int OpenIncidents,
    int CriticalIncidents,
    int OpenCAPAs,
    int OverdueCAPAs,
    int ScheduledAudits,
    int OpenFindings,
    decimal OverallComplianceRate,
    List<IncidentByCategory> IncidentsByCategory,
    List<MetricSummary> KeyMetrics
);

public record IncidentByCategory(
    IncidentCategory Category,
    int Count,
    int Open,
    int Closed
);

public record MetricSummary(
    string MetricName,
    decimal CurrentValue,
    decimal? TargetValue,
    MetricStatus Status,
    string? Unit
);

public record IncidentTrend(
    DateTime Period,
    int TotalIncidents,
    int CriticalIncidents,
    int PatientSafetyIncidents,
    decimal AverageResolutionDays
);

public record CAPAAnalysis(
    CAPAType Type,
    int Total,
    int Completed,
    int Overdue,
    decimal AverageCompletionDays,
    decimal EffectivenessRate
);
