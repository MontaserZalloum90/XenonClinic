using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Advanced HR service covering payroll, recruitment, and training.
/// </summary>
public interface IAdvancedHRService
{
    // Payroll
    Task<PayrollRun> CreatePayrollRunAsync(CreatePayrollRunRequest request);
    Task<PayrollRun?> GetPayrollRunAsync(int payrollRunId);
    Task<IEnumerable<PayrollRun>> GetPayrollRunsAsync(int year, int? month = null);
    Task<PayrollRun> CalculatePayrollAsync(int payrollRunId);
    Task<PayrollRun> ApprovePayrollAsync(int payrollRunId);
    Task ProcessPayrollAsync(int payrollRunId);
    Task<PayrollEntry?> GetPayrollEntryAsync(int entryId);
    Task<IEnumerable<PayrollEntry>> GetEmployeePayrollHistoryAsync(int employeeId, int? year = null);
    Task<PaySlip> GeneratePaySlipAsync(int payrollEntryId);
    Task AddPayrollAdjustmentAsync(int payrollEntryId, AddPayrollAdjustmentRequest request);

    // Recruitment
    Task<JobRequisition> CreateRequisitionAsync(CreateRequisitionRequest request);
    Task<JobRequisition?> GetRequisitionAsync(int requisitionId);
    Task<IEnumerable<JobRequisition>> GetRequisitionsAsync(RequisitionSearchCriteria criteria);
    Task<JobRequisition> UpdateRequisitionAsync(int requisitionId, UpdateRequisitionRequest request);
    Task ApproveRequisitionAsync(int requisitionId);
    Task CloseRequisitionAsync(int requisitionId, string reason);

    Task<JobCandidate> AddCandidateAsync(AddCandidateRequest request);
    Task<JobCandidate?> GetCandidateAsync(int candidateId);
    Task<IEnumerable<JobCandidate>> GetCandidatesAsync(CandidateSearchCriteria criteria);
    Task<JobCandidate> UpdateCandidateStatusAsync(int candidateId, CandidateStatus status, string? notes = null);
    Task<CandidateInterview> ScheduleInterviewAsync(ScheduleInterviewRequest request);
    Task<CandidateInterview> RecordInterviewFeedbackAsync(int interviewId, RecordInterviewFeedbackRequest request);
    Task HireCandidateAsync(int candidateId, HireCandidateRequest request);
    Task RejectCandidateAsync(int candidateId, string reason);

    // Training
    Task<TrainingProgram> CreateTrainingProgramAsync(CreateTrainingProgramRequest request);
    Task<TrainingProgram?> GetTrainingProgramAsync(int programId);
    Task<IEnumerable<TrainingProgram>> GetTrainingProgramsAsync(string? category = null, bool activeOnly = true);
    Task<TrainingProgram> UpdateTrainingProgramAsync(int programId, UpdateTrainingProgramRequest request);

    Task<TrainingSession> CreateTrainingSessionAsync(CreateTrainingSessionRequest request);
    Task<TrainingSession?> GetTrainingSessionAsync(int sessionId);
    Task<IEnumerable<TrainingSession>> GetUpcomingSessionsAsync(int? programId = null);
    Task<TrainingEnrollment> EnrollEmployeeAsync(int sessionId, int employeeId);
    Task<TrainingEnrollment> RecordTrainingCompletionAsync(int enrollmentId, RecordTrainingCompletionRequest request);
    Task CancelEnrollmentAsync(int enrollmentId, string reason);

    Task<IEnumerable<EmployeeCertification>> GetEmployeeCertificationsAsync(int employeeId);
    Task<EmployeeCertification> AddCertificationAsync(AddCertificationRequest request);
    Task<IEnumerable<EmployeeCertification>> GetExpiringCertificationsAsync(int daysAhead = 30);
    Task RenewCertificationAsync(int certificationId, DateTime newExpiryDate);

    // Loans
    Task<EmployeeLoan> CreateLoanAsync(CreateLoanRequest request);
    Task<EmployeeLoan?> GetLoanAsync(int loanId);
    Task<IEnumerable<EmployeeLoan>> GetEmployeeLoansAsync(int employeeId);
    Task<LoanPayment> RecordLoanPaymentAsync(int loanId, decimal amount, int? payrollEntryId = null);
    Task<decimal> GetTotalLoanDeductionAsync(int employeeId);

    // Reporting
    Task<PayrollSummary> GetPayrollSummaryAsync(int year, int month);
    Task<RecruitmentDashboard> GetRecruitmentDashboardAsync();
    Task<TrainingDashboard> GetTrainingDashboardAsync();
    Task<IEnumerable<EmployeeTrainingRecord>> GetEmployeeTrainingRecordAsync(int employeeId);
}

// Request DTOs
public record CreatePayrollRunRequest(
    int Year,
    int Month,
    PayrollPeriodType PeriodType,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime PayDate,
    int BranchId
);

public record AddPayrollAdjustmentRequest(
    string Type, // Earning or Deduction
    string Description,
    decimal Amount,
    bool IsTaxable = true
);

public record CreateRequisitionRequest(
    string Title,
    int? JobPositionId,
    int DepartmentId,
    RequisitionType Type,
    RequisitionPriority Priority,
    int NumberOfPositions,
    EmploymentType EmploymentType,
    string? Location,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? Description,
    string? Requirements,
    DateTime? TargetHireDate
);

public record UpdateRequisitionRequest(
    string? Title,
    string? Description,
    string? Requirements,
    decimal? SalaryMin,
    decimal? SalaryMax,
    DateTime? TargetHireDate,
    RequisitionPriority? Priority
);

public record RequisitionSearchCriteria(
    RequisitionStatus? Status = null,
    int? DepartmentId = null,
    RequisitionType? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 50
);

public record AddCandidateRequest(
    int? RequisitionId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    CandidateSource Source,
    string? SourceDetail,
    string? CurrentCompany,
    string? CurrentTitle,
    int? YearsOfExperience,
    decimal? ExpectedSalary,
    string? ResumeFilePath
);

public record CandidateSearchCriteria(
    int? RequisitionId = null,
    CandidateStatus? Status = null,
    CandidateSource? Source = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

public record ScheduleInterviewRequest(
    int CandidateId,
    InterviewType Type,
    DateTime ScheduledDate,
    int DurationMinutes,
    string? Location,
    string? MeetingLink,
    List<int> InterviewerEmployeeIds
);

public record RecordInterviewFeedbackRequest(
    int OverallRating,
    string Feedback,
    string? Strengths,
    string? Weaknesses,
    string Recommendation
);

public record HireCandidateRequest(
    int JobPositionId,
    int DepartmentId,
    DateTime StartDate,
    decimal Salary,
    EmploymentType EmploymentType
);

public record CreateTrainingProgramRequest(
    string Code,
    string Name,
    string? Description,
    TrainingType Type,
    string? Category,
    int DurationHours,
    string? Provider,
    bool IsExternal,
    bool IsMandatory,
    bool RequiresCertification,
    int? CertificationValidityMonths,
    decimal? Cost
);

public record UpdateTrainingProgramRequest(
    string? Name,
    string? Description,
    int? DurationHours,
    decimal? Cost,
    bool? IsActive
);

public record CreateTrainingSessionRequest(
    int TrainingProgramId,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    string? VirtualLink,
    TrainingDeliveryMode DeliveryMode,
    int? MaxParticipants,
    int? InstructorEmployeeId,
    int BranchId
);

public record RecordTrainingCompletionRequest(
    decimal? Score,
    bool Passed,
    string? CertificateNumber,
    DateTime? CertificateExpiryDate,
    string? Feedback,
    int? Rating
);

public record AddCertificationRequest(
    int EmployeeId,
    string CertificationName,
    string? CertificationNumber,
    string? IssuingOrganization,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    bool NeverExpires,
    string? DocumentFilePath,
    bool IsRequired,
    int? TrainingEnrollmentId
);

public record CreateLoanRequest(
    int EmployeeId,
    LoanType Type,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    DateTime DisbursementDate,
    DateTime FirstDeductionDate,
    string? Purpose
);

// Response DTOs
public record PaySlip(
    string EmployeeName,
    string EmployeeId,
    string Department,
    string Position,
    string PayPeriod,
    DateTime PayDate,
    List<PaySlipEarning> Earnings,
    decimal TotalEarnings,
    List<PaySlipDeduction> Deductions,
    decimal TotalDeductions,
    decimal NetPay,
    decimal YTDGross,
    decimal YTDTax,
    decimal YTDNet
);

public record PaySlipEarning(string Description, decimal Amount, decimal YTD);
public record PaySlipDeduction(string Description, decimal Amount, decimal YTD);

public record PayrollSummary(
    int Year,
    int Month,
    int EmployeeCount,
    decimal TotalGrossPay,
    decimal TotalDeductions,
    decimal TotalNetPay,
    decimal TotalEmployerContributions,
    Dictionary<string, decimal> EarningsByType,
    Dictionary<string, decimal> DeductionsByType
);

public record RecruitmentDashboard(
    int OpenRequisitions,
    int TotalCandidates,
    int CandidatesInPipeline,
    int InterviewsThisWeek,
    int HiredThisMonth,
    decimal AverageTimeToHire,
    List<RequisitionSummary> RecentRequisitions,
    Dictionary<CandidateStatus, int> CandidatesByStatus,
    Dictionary<CandidateSource, int> CandidatesBySource
);

public record RequisitionSummary(
    int Id,
    string Title,
    string Department,
    RequisitionStatus Status,
    int CandidateCount,
    DateTime? TargetHireDate
);

public record TrainingDashboard(
    int ActivePrograms,
    int UpcomingSessions,
    int TotalEnrollments,
    int CompletionsThisMonth,
    decimal AverageRating,
    int ExpiringCertifications,
    List<TrainingSessionSummary> UpcomingSessionsList,
    Dictionary<string, int> EnrollmentsByCategory
);

public record TrainingSessionSummary(
    int SessionId,
    string ProgramName,
    DateTime StartDate,
    int EnrolledCount,
    int MaxParticipants,
    string? Location
);

public record EmployeeTrainingRecord(
    string ProgramName,
    string SessionCode,
    DateTime CompletedDate,
    decimal? Score,
    bool Passed,
    string? CertificateNumber,
    DateTime? CertificateExpiry
);
