namespace XenonClinic.Core.Entities;

/// <summary>
/// Quality incident/event report.
/// </summary>
public class QualityIncident : AuditableEntityWithId
{
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IncidentCategory Category { get; set; }
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Reported;

    public DateTime IncidentDate { get; set; }
    public DateTime ReportedDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public int? ReportedByEmployeeId { get; set; }

    public int? PatientId { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public string? Location { get; set; }

    public string? ImmediateActions { get; set; }
    public string? RootCause { get; set; }
    public string? ContributingFactors { get; set; }

    public string? AssignedTo { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosedBy { get; set; }
    public string? ClosureNotes { get; set; }

    public bool RequiresRootCauseAnalysis { get; set; }
    public bool IsReportable { get; set; }
    public string? RegulatoryReportNumber { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
    public virtual ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();
    public virtual ICollection<IncidentNote> Notes { get; set; } = new List<IncidentNote>();
}

/// <summary>
/// Corrective/Preventive action (CAPA).
/// </summary>
public class CorrectiveAction : AuditableEntityWithId
{
    public string ActionNumber { get; set; } = string.Empty;
    public int? IncidentId { get; set; }
    public int? AuditFindingId { get; set; }
    public int? ComplaintId { get; set; }

    public CAPAType Type { get; set; }
    public CAPAStatus Status { get; set; } = CAPAStatus.Open;
    public CAPAPriority Priority { get; set; } = CAPAPriority.Medium;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RootCause { get; set; }

    public string? ProposedAction { get; set; }
    public string? ActionTaken { get; set; }
    public string? VerificationMethod { get; set; }
    public string? EffectivenessReview { get; set; }
    public bool? IsEffective { get; set; }

    public string? AssignedTo { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public string? VerifiedBy { get; set; }

    public int? DepartmentId { get; set; }
    public string? ProcessAffected { get; set; }

    public int CompanyId { get; set; }

    public virtual QualityIncident? Incident { get; set; }
    public virtual ICollection<CAPATask> Tasks { get; set; } = new List<CAPATask>();
}

/// <summary>
/// Task within a CAPA.
/// </summary>
public class CAPATask : AuditableEntityWithId
{
    public int CorrectiveActionId { get; set; }
    public int Sequence { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string? Notes { get; set; }

    public virtual CorrectiveAction? CorrectiveAction { get; set; }
}

/// <summary>
/// Quality audit.
/// </summary>
public class QualityAudit : AuditableEntityWithId
{
    public string AuditNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AuditType Type { get; set; }
    public AuditStatus Status { get; set; } = AuditStatus.Planned;

    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? Scope { get; set; }
    public string? Objectives { get; set; }
    public string? Criteria { get; set; }
    public int? DepartmentId { get; set; }
    public string? ProcessAudited { get; set; }

    public string? LeadAuditor { get; set; }
    public int? LeadAuditorEmployeeId { get; set; }
    public string? AuditTeam { get; set; }

    public string? Findings { get; set; }
    public string? Observations { get; set; }
    public string? Recommendations { get; set; }
    public string? Conclusion { get; set; }

    public int NonConformities { get; set; }
    public int Observations_ { get; set; }
    public int Opportunities { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<AuditFinding> AuditFindings { get; set; } = new List<AuditFinding>();
    public virtual ICollection<AuditChecklistItem> ChecklistItems { get; set; } = new List<AuditChecklistItem>();
}

/// <summary>
/// Finding from a quality audit.
/// </summary>
public class AuditFinding : AuditableEntityWithId
{
    public int AuditId { get; set; }
    public string FindingNumber { get; set; } = string.Empty;

    public FindingType Type { get; set; }
    public FindingSeverity Severity { get; set; }
    public FindingStatus Status { get; set; } = FindingStatus.Open;

    public string Description { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public string? CriteriaReference { get; set; }
    public string? RootCause { get; set; }

    public string? Response { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public int? CorrectiveActionId { get; set; }

    public virtual QualityAudit? Audit { get; set; }
    public virtual CorrectiveAction? CorrectiveAction { get; set; }
}

/// <summary>
/// Checklist item for audit.
/// </summary>
public class AuditChecklistItem : AuditableEntityWithId
{
    public int AuditId { get; set; }
    public int? ChecklistTemplateItemId { get; set; }
    public int Sequence { get; set; }

    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? Criteria { get; set; }

    public ChecklistResult? Result { get; set; }
    public string? Notes { get; set; }
    public string? Evidence { get; set; }

    public virtual QualityAudit? Audit { get; set; }
}

/// <summary>
/// Quality metric/KPI.
/// </summary>
public class QualityMetric : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }

    public MetricType Type { get; set; }
    public string? Formula { get; set; }
    public string? Unit { get; set; }

    public decimal? TargetValue { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public TrendDirection DesiredTrend { get; set; }

    public string? DataSource { get; set; }
    public string? Frequency { get; set; } // Daily, Weekly, Monthly
    public bool IsActive { get; set; } = true;

    public int? DepartmentId { get; set; }
    public int CompanyId { get; set; }

    public virtual ICollection<QualityMetricValue> Values { get; set; } = new List<QualityMetricValue>();
}

/// <summary>
/// Recorded value for a quality metric.
/// </summary>
public class QualityMetricValue : AuditableEntityWithId
{
    public int MetricId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public decimal Value { get; set; }
    public decimal? Numerator { get; set; }
    public decimal? Denominator { get; set; }

    public MetricStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? DataSourceDetails { get; set; }

    public virtual QualityMetric? Metric { get; set; }
}

/// <summary>
/// Compliance checklist template.
/// </summary>
public class ComplianceChecklist : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Standard { get; set; } // JCI, HIPAA, etc.
    public string? Version { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }

    public virtual ICollection<ComplianceChecklistItem> Items { get; set; } = new List<ComplianceChecklistItem>();
}

/// <summary>
/// Item in a compliance checklist.
/// </summary>
public class ComplianceChecklistItem : AuditableEntityWithId
{
    public int ChecklistId { get; set; }
    public int Sequence { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public string? Guidance { get; set; }
    public string? EvidenceRequired { get; set; }
    public bool IsMandatory { get; set; } = true;

    public virtual ComplianceChecklist? Checklist { get; set; }
}

/// <summary>
/// Attachment to an incident.
/// </summary>
public class IncidentAttachment : AuditableEntityWithId
{
    public int IncidentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }

    public virtual QualityIncident? Incident { get; set; }
}

/// <summary>
/// Note on an incident.
/// </summary>
public class IncidentNote : AuditableEntityWithId
{
    public int IncidentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? NoteType { get; set; }

    public virtual QualityIncident? Incident { get; set; }
}

// Enums
public enum IncidentCategory
{
    PatientSafety = 1,
    MedicationError = 2,
    FallEvent = 3,
    InfectionControl = 4,
    EquipmentFailure = 5,
    SecurityBreach = 6,
    DataPrivacy = 7,
    StaffInjury = 8,
    PropertyDamage = 9,
    NearMiss = 10,
    Complaint = 11,
    Other = 99
}

public enum IncidentSeverity
{
    NoHarm = 1,
    Minor = 2,
    Moderate = 3,
    Major = 4,
    Catastrophic = 5
}

public enum IncidentStatus
{
    Reported = 1,
    UnderReview = 2,
    InvestigationInProgress = 3,
    PendingAction = 4,
    ActionInProgress = 5,
    Closed = 6,
    Reopened = 7
}

public enum CAPAType
{
    Corrective = 1,
    Preventive = 2,
    Both = 3
}

public enum CAPAStatus
{
    Open = 1,
    InProgress = 2,
    PendingVerification = 3,
    Verified = 4,
    Closed = 5,
    Rejected = 6
}

public enum CAPAPriority
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    OnHold = 5
}

public enum AuditType
{
    Internal = 1,
    External = 2,
    Surveillance = 3,
    Certification = 4,
    Regulatory = 5,
    Supplier = 6
}

public enum AuditStatus
{
    Planned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Postponed = 5
}

public enum FindingType
{
    NonConformity = 1,
    Observation = 2,
    OpportunityForImprovement = 3,
    Positive = 4
}

public enum FindingSeverity
{
    Major = 1,
    Minor = 2,
    Observation = 3
}

public enum FindingStatus
{
    Open = 1,
    ResponsePending = 2,
    ActionInProgress = 3,
    Closed = 4
}

public enum ChecklistResult
{
    Compliant = 1,
    NonCompliant = 2,
    PartiallyCompliant = 3,
    NotApplicable = 4,
    NotAudited = 5
}

public enum MetricType
{
    Count = 1,
    Percentage = 2,
    Rate = 3,
    Average = 4,
    Sum = 5,
    Ratio = 6
}

public enum TrendDirection
{
    Higher = 1,
    Lower = 2,
    Stable = 3
}

public enum MetricStatus
{
    OnTarget = 1,
    BelowTarget = 2,
    AboveTarget = 3,
    Critical = 4
}
