namespace XenonClinic.Core.Entities;

/// <summary>
/// Payroll run/batch.
/// </summary>
public class PayrollRun : AuditableEntityWithId
{
    public string PayrollNumber { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollPeriodType PeriodType { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? ProcessedDate { get; set; }
    public string? ProcessedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }

    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public int EmployeeCount { get; set; }

    public string? Notes { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<PayrollEntry> Entries { get; set; } = new List<PayrollEntry>();
}

/// <summary>
/// Individual employee payroll entry.
/// </summary>
public class PayrollEntry : AuditableEntityWithId
{
    public int PayrollRunId { get; set; }
    public int EmployeeId { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal Bonus { get; set; }
    public decimal Commission { get; set; }
    public decimal GrossPay { get; set; }

    // Deductions
    public decimal TaxWithholding { get; set; }
    public decimal SocialSecurity { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal PensionContribution { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    public decimal NetPay { get; set; }

    // Employer contributions
    public decimal EmployerSocialSecurity { get; set; }
    public decimal EmployerPension { get; set; }
    public decimal EmployerHealthInsurance { get; set; }
    public decimal TotalEmployerContributions { get; set; }

    // Work details
    public int WorkingDays { get; set; }
    public int AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public int LeaveDaysTaken { get; set; }

    public string? PaymentMethod { get; set; }
    public string? BankAccount { get; set; }
    public string? Notes { get; set; }

    public virtual PayrollRun? PayrollRun { get; set; }
    public virtual ICollection<PayrollEarning> Earnings { get; set; } = new List<PayrollEarning>();
    public virtual ICollection<PayrollDeduction> Deductions { get; set; } = new List<PayrollDeduction>();
}

/// <summary>
/// Detailed earning item.
/// </summary>
public class PayrollEarning : AuditableEntityWithId
{
    public int PayrollEntryId { get; set; }
    public string EarningType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; } = true;

    public virtual PayrollEntry? PayrollEntry { get; set; }
}

/// <summary>
/// Detailed deduction item.
/// </summary>
public class PayrollDeduction : AuditableEntityWithId
{
    public int PayrollEntryId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public bool IsPreTax { get; set; }

    public virtual PayrollEntry? PayrollEntry { get; set; }
}

/// <summary>
/// Job requisition for recruitment.
/// </summary>
public class JobRequisition : AuditableEntityWithId
{
    public string RequisitionNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? JobPositionId { get; set; }
    public int DepartmentId { get; set; }
    public int? ReportingToEmployeeId { get; set; }

    public RequisitionType Type { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public RequisitionPriority Priority { get; set; } = RequisitionPriority.Normal;

    public int NumberOfPositions { get; set; } = 1;
    public EmploymentType EmploymentType { get; set; }
    public string? Location { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Currency { get; set; }

    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Benefits { get; set; }

    public DateTime RequestedDate { get; set; }
    public DateTime? TargetHireDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string? ClosureReason { get; set; }

    public string? RequestedBy { get; set; }
    public int? RequestedByEmployeeId { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<JobCandidate> Candidates { get; set; } = new List<JobCandidate>();
}

/// <summary>
/// Job candidate/applicant.
/// </summary>
public class JobCandidate : AuditableEntityWithId
{
    public int? RequisitionId { get; set; }
    public string CandidateNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public CandidateStatus Status { get; set; } = CandidateStatus.New;
    public CandidateSource Source { get; set; }
    public string? SourceDetail { get; set; }

    public string? CurrentCompany { get; set; }
    public string? CurrentTitle { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? HighestEducation { get; set; }

    public decimal? ExpectedSalary { get; set; }
    public string? Currency { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public int? NoticePeriodDays { get; set; }

    public string? ResumeFilePath { get; set; }
    public string? CoverLetterFilePath { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }

    public string? Skills { get; set; }
    public string? Notes { get; set; }
    public int? Rating { get; set; }

    public string? RejectionReason { get; set; }
    public DateTime? HiredDate { get; set; }
    public int? HiredAsEmployeeId { get; set; }

    public int CompanyId { get; set; }

    public virtual JobRequisition? Requisition { get; set; }
    public virtual ICollection<CandidateInterview> Interviews { get; set; } = new List<CandidateInterview>();
    public virtual ICollection<CandidateNote> CandidateNotes { get; set; } = new List<CandidateNote>();
}

/// <summary>
/// Interview scheduled for candidate.
/// </summary>
public class CandidateInterview : AuditableEntityWithId
{
    public int CandidateId { get; set; }
    public InterviewType Type { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public DateTime ScheduledDate { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }

    public string? InterviewerIds { get; set; } // JSON array
    public string? InterviewerNames { get; set; }

    public int? OverallRating { get; set; }
    public string? Feedback { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public string? Recommendation { get; set; } // Hire, Reject, NextRound

    public virtual JobCandidate? Candidate { get; set; }
}

/// <summary>
/// Note on candidate.
/// </summary>
public class CandidateNote : AuditableEntityWithId
{
    public int CandidateId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? NoteType { get; set; }

    public virtual JobCandidate? Candidate { get; set; }
}

/// <summary>
/// Training program.
/// </summary>
public class TrainingProgram : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TrainingType Type { get; set; }
    public string? Category { get; set; }

    public int DurationHours { get; set; }
    public string? Provider { get; set; }
    public bool IsExternal { get; set; }
    public bool IsMandatory { get; set; }
    public bool RequiresCertification { get; set; }
    public int? CertificationValidityMonths { get; set; }

    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
    public string? Prerequisites { get; set; }
    public string? Objectives { get; set; }
    public string? Syllabus { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }

    public virtual ICollection<TrainingSession> Sessions { get; set; } = new List<TrainingSession>();
}

/// <summary>
/// Training session instance.
/// </summary>
public class TrainingSession : AuditableEntityWithId
{
    public int TrainingProgramId { get; set; }
    public string SessionCode { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? VirtualLink { get; set; }
    public TrainingDeliveryMode DeliveryMode { get; set; }

    public int? MaxParticipants { get; set; }
    public int EnrolledCount { get; set; }

    public string? InstructorName { get; set; }
    public int? InstructorEmployeeId { get; set; }

    public TrainingSessionStatus Status { get; set; } = TrainingSessionStatus.Scheduled;
    public string? Notes { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual TrainingProgram? TrainingProgram { get; set; }
    public virtual ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

/// <summary>
/// Employee training enrollment.
/// </summary>
public class TrainingEnrollment : AuditableEntityWithId
{
    public int TrainingSessionId { get; set; }
    public int EmployeeId { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public decimal? Score { get; set; }
    public bool? Passed { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateExpiryDate { get; set; }

    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public string? Notes { get; set; }

    public virtual TrainingSession? TrainingSession { get; set; }
}

/// <summary>
/// Employee certification/credential.
/// </summary>
public class EmployeeCertification : AuditableEntityWithId
{
    public int EmployeeId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string? CertificationNumber { get; set; }
    public string? IssuingOrganization { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool NeverExpires { get; set; }

    public CertificationStatus Status { get; set; } = CertificationStatus.Active;
    public string? DocumentFilePath { get; set; }
    public string? Notes { get; set; }

    public int? TrainingEnrollmentId { get; set; }
    public bool IsRequired { get; set; }

    public int CompanyId { get; set; }
}

/// <summary>
/// Employee loan.
/// </summary>
public class EmployeeLoan : AuditableEntityWithId
{
    public int EmployeeId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public LoanType Type { get; set; }

    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MonthlyDeduction { get; set; }

    public DateTime ApprovalDate { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime FirstDeductionDate { get; set; }

    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public string? ApprovedBy { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }

    public int CompanyId { get; set; }

    public virtual ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();
}

/// <summary>
/// Loan payment/deduction record.
/// </summary>
public class LoanPayment : AuditableEntityWithId
{
    public int EmployeeLoanId { get; set; }
    public int PaymentNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public int? PayrollEntryId { get; set; }

    public virtual EmployeeLoan? Loan { get; set; }
}

// Enums
public enum PayrollPeriodType
{
    Monthly = 1,
    BiWeekly = 2,
    Weekly = 3,
    SemiMonthly = 4
}

public enum PayrollStatus
{
    Draft = 1,
    Calculated = 2,
    PendingApproval = 3,
    Approved = 4,
    Processing = 5,
    Paid = 6,
    Cancelled = 7
}

public enum RequisitionType
{
    NewPosition = 1,
    Replacement = 2,
    Expansion = 3,
    Contract = 4,
    Internship = 5
}

public enum RequisitionStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Open = 4,
    OnHold = 5,
    Filled = 6,
    Cancelled = 7
}

public enum RequisitionPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Temporary = 4,
    Internship = 5
}

public enum CandidateStatus
{
    New = 1,
    Screening = 2,
    Interview = 3,
    Assessment = 4,
    Reference = 5,
    Offer = 6,
    Hired = 7,
    Rejected = 8,
    Withdrawn = 9,
    OnHold = 10
}

public enum CandidateSource
{
    JobBoard = 1,
    CompanyWebsite = 2,
    Referral = 3,
    LinkedIn = 4,
    Agency = 5,
    University = 6,
    Internal = 7,
    Other = 8
}

public enum InterviewType
{
    Phone = 1,
    Video = 2,
    InPerson = 3,
    Technical = 4,
    Panel = 5,
    HR = 6,
    Final = 7
}

public enum InterviewStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4,
    Rescheduled = 5
}

public enum TrainingType
{
    Orientation = 1,
    Technical = 2,
    SoftSkills = 3,
    Compliance = 4,
    Safety = 5,
    Leadership = 6,
    Certification = 7,
    Other = 8
}

public enum TrainingDeliveryMode
{
    InPerson = 1,
    Virtual = 2,
    Hybrid = 3,
    SelfPaced = 4
}

public enum TrainingSessionStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Postponed = 5
}

public enum EnrollmentStatus
{
    Enrolled = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Withdrawn = 5,
    NoShow = 6
}

public enum CertificationStatus
{
    Active = 1,
    Expired = 2,
    Revoked = 3,
    Pending = 4
}

public enum LoanType
{
    Personal = 1,
    Emergency = 2,
    Housing = 3,
    Education = 4,
    Medical = 5,
    Vehicle = 6,
    Other = 7
}

public enum LoanStatus
{
    Pending = 1,
    Active = 2,
    PaidOff = 3,
    Defaulted = 4,
    WrittenOff = 5
}
