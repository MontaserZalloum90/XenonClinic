namespace XenonClinic.Core.DTOs;

#region Payroll Period DTOs

/// <summary>
/// Payroll period DTO
/// </summary>
public class PayrollPeriodDto
{
    public int Id { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty; // Draft, Processing, Approved, Paid, Closed
    public int TotalEmployees { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

/// <summary>
/// Create payroll period request
/// </summary>
public class CreatePayrollPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Payslip DTOs

/// <summary>
/// Employee payslip DTO
/// </summary>
public class PayslipDto
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime PaymentDate { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal FoodAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Overtime { get; set; }
    public decimal Bonus { get; set; }
    public decimal Commission { get; set; }
    public List<PayslipEarningDto> Earnings { get; set; } = new();
    public decimal TotalEarnings { get; set; }

    // Deductions
    public decimal SocialInsurance { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal Tax { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public List<PayslipDeductionDto> Deductions { get; set; } = new();
    public decimal TotalDeductions { get; set; }

    // Net Pay
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }

    // Bank Details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IBAN { get; set; }

    // Leave Summary
    public int AnnualLeaveBalance { get; set; }
    public int SickLeaveBalance { get; set; }
    public int DaysWorked { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDaysTaken { get; set; }

    // Status
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
}

/// <summary>
/// Payslip earning line item
/// </summary>
public class PayslipEarningDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; }
}

/// <summary>
/// Payslip deduction line item
/// </summary>
public class PayslipDeductionDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsMandatory { get; set; }
}

/// <summary>
/// Payslip summary for list view
/// </summary>
public class PayslipSummaryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}

#endregion

#region WPS (Wage Protection System) DTOs

/// <summary>
/// WPS file generation request
/// </summary>
public class WpsGenerationRequestDto
{
    public int PayrollPeriodId { get; set; }
    public string FileFormat { get; set; } = "SIF"; // SIF, MOL, Custom
    public DateTime PaymentDate { get; set; }
    public string? PayerBankCode { get; set; }
    public string? PayerAccountNumber { get; set; }
    public string? PayerIBAN { get; set; }
    public List<int>? EmployeeIds { get; set; } // null = all employees
}

/// <summary>
/// WPS file response
/// </summary>
public class WpsFileResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string FileContent { get; set; } = string.Empty; // Base64 encoded
    public string? ContentType { get; set; }
    public List<WpsRecordDto> Records { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
}

/// <summary>
/// Individual WPS record
/// </summary>
public class WpsRecordDto
{
    public string RecordType { get; set; } = "SAL"; // SAL = Salary
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmiratesId { get; set; }
    public string? LaborCardNumber { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public string? RoutingCode { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public DateTime PaymentDate { get; set; }
    public string? Narration { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// WPS submission status
/// </summary>
public class WpsSubmissionDto
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Submitted, Accepted, Rejected, Partially Processed
    public string? ReferenceNumber { get; set; }
    public int? ProcessedRecords { get; set; }
    public int? RejectedRecords { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

#endregion

#region Payroll Report DTOs

/// <summary>
/// Payroll summary report
/// </summary>
public class PayrollSummaryReportDto
{
    public string ReportTitle { get; set; } = "Payroll Summary Report";
    public DateTime GeneratedDate { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int NewHires { get; set; }
    public int Terminations { get; set; }

    public decimal TotalBasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalOvertime { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalGrossPay { get; set; }

    public decimal TotalSocialInsurance { get; set; }
    public decimal TotalHealthInsurance { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalOtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    public decimal TotalNetPay { get; set; }
    public decimal AverageNetPay { get; set; }
    public decimal MedianNetPay { get; set; }
    public decimal HighestNetPay { get; set; }
    public decimal LowestNetPay { get; set; }

    public List<PayrollDepartmentSummaryDto> ByDepartment { get; set; } = new();
    public List<PayrollPaymentMethodSummaryDto> ByPaymentMethod { get; set; } = new();
}

/// <summary>
/// Payroll summary by department
/// </summary>
public class PayrollDepartmentSummaryDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public decimal AverageNetPay { get; set; }
}

/// <summary>
/// Payroll summary by payment method
/// </summary>
public class PayrollPaymentMethodSummaryDto
{
    public string PaymentMethod { get; set; } = string.Empty; // Bank Transfer, Cash, Check
    public int EmployeeCount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Detailed payroll register report
/// </summary>
public class PayrollRegisterReportDto
{
    public string ReportTitle { get; set; } = "Payroll Register";
    public DateTime GeneratedDate { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public List<PayrollRegisterEntryDto> Entries { get; set; } = new();
    public PayrollRegisterTotalsDto Totals { get; set; } = new();
}

/// <summary>
/// Individual payroll register entry
/// </summary>
public class PayrollRegisterEntryDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public DateTime JoinDate { get; set; }
    public int DaysWorked { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Overtime { get; set; }
    public decimal GrossPay { get; set; }
    public decimal SocialInsurance { get; set; }
    public decimal Tax { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
}

/// <summary>
/// Payroll register totals
/// </summary>
public class PayrollRegisterTotalsDto
{
    public int TotalEmployees { get; set; }
    public decimal TotalBasicSalary { get; set; }
    public decimal TotalHousingAllowance { get; set; }
    public decimal TotalTransportAllowance { get; set; }
    public decimal TotalOtherAllowances { get; set; }
    public decimal TotalOvertime { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalSocialInsurance { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalOtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
}

/// <summary>
/// Bank transfer list report for payroll
/// </summary>
public class BankTransferListReportDto
{
    public string ReportTitle { get; set; } = "Bank Transfer List";
    public DateTime GeneratedDate { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public string PayerBankName { get; set; } = string.Empty;
    public string PayerAccountNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int TotalTransfers { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BankTransferEntryDto> Transfers { get; set; } = new();
}

/// <summary>
/// Individual bank transfer entry
/// </summary>
public class BankTransferEntryDto
{
    public int SequenceNumber { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? IBAN { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public string? Purpose { get; set; }
}

/// <summary>
/// Year-to-date earnings report
/// </summary>
public class YtdEarningsReportDto
{
    public string ReportTitle { get; set; } = "Year-to-Date Earnings Report";
    public DateTime GeneratedDate { get; set; }
    public int Year { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }

    public decimal YtdBasicSalary { get; set; }
    public decimal YtdAllowances { get; set; }
    public decimal YtdOvertime { get; set; }
    public decimal YtdBonus { get; set; }
    public decimal YtdGrossPay { get; set; }

    public decimal YtdSocialInsurance { get; set; }
    public decimal YtdHealthInsurance { get; set; }
    public decimal YtdTax { get; set; }
    public decimal YtdOtherDeductions { get; set; }
    public decimal YtdTotalDeductions { get; set; }

    public decimal YtdNetPay { get; set; }

    public List<MonthlyEarningsSummaryDto> MonthlyBreakdown { get; set; } = new();
}

/// <summary>
/// Monthly earnings summary
/// </summary>
public class MonthlyEarningsSummaryDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
}

/// <summary>
/// Employee cost report
/// </summary>
public class EmployeeCostReportDto
{
    public string ReportTitle { get; set; } = "Employee Cost Report";
    public DateTime GeneratedDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    public decimal TotalSalaryCost { get; set; }
    public decimal TotalAllowancesCost { get; set; }
    public decimal TotalBenefitsCost { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public decimal TotalEmployeeCost { get; set; }

    public decimal AverageCostPerEmployee { get; set; }
    public decimal CostPerFTE { get; set; }

    public List<DepartmentCostDto> ByDepartment { get; set; } = new();
    public List<EmployeeCostEntryDto> TopCostEmployees { get; set; } = new();
}

/// <summary>
/// Department cost breakdown
/// </summary>
public class DepartmentCostDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int HeadCount { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// Individual employee cost entry
/// </summary>
public class EmployeeCostEntryDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public decimal Salary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Benefits { get; set; }
    public decimal EmployerContributions { get; set; }
    public decimal TotalCost { get; set; }
}

/// <summary>
/// Payroll variance report
/// </summary>
public class PayrollVarianceReportDto
{
    public string ReportTitle { get; set; } = "Payroll Variance Report";
    public DateTime GeneratedDate { get; set; }
    public string CurrentPeriod { get; set; } = string.Empty;
    public string PreviousPeriod { get; set; } = string.Empty;

    public decimal CurrentTotalPayroll { get; set; }
    public decimal PreviousTotalPayroll { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }

    public List<PayrollVarianceEntryDto> SignificantVariances { get; set; } = new();
    public List<PayrollCategoryVarianceDto> CategoryVariances { get; set; } = new();
}

/// <summary>
/// Individual variance entry
/// </summary>
public class PayrollVarianceEntryDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal PreviousAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Category variance
/// </summary>
public class PayrollCategoryVarianceDto
{
    public string Category { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal PreviousAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
}

#endregion

#region Payroll Configuration DTOs

/// <summary>
/// Salary component configuration
/// </summary>
public class SalaryComponentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string ComponentType { get; set; } = string.Empty; // Earning, Deduction
    public string Category { get; set; } = string.Empty; // Basic, Allowance, Bonus, Tax, Insurance
    public bool IsTaxable { get; set; }
    public bool IsPartOfGross { get; set; }
    public string CalculationType { get; set; } = string.Empty; // Fixed, Percentage, Formula
    public decimal? DefaultValue { get; set; }
    public string? Formula { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// Tax configuration
/// </summary>
public class TaxConfigurationDto
{
    public int Id { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public bool IsActive { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

#endregion

#region Payroll Request DTOs

/// <summary>
/// Payroll processing request
/// </summary>
public class ProcessPayrollRequestDto
{
    public int PayrollPeriodId { get; set; }
    public List<int>? EmployeeIds { get; set; } // null = all employees
    public bool RecalculateAll { get; set; }
    public bool IncludeLeaveAdjustments { get; set; } = true;
    public bool IncludeOvertimeCalculations { get; set; } = true;
}

/// <summary>
/// Payroll approval request
/// </summary>
public class ApprovePayrollRequestDto
{
    public int PayrollPeriodId { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

/// <summary>
/// Payroll report request
/// </summary>
public class PayrollReportRequestDto
{
    public int BranchId { get; set; }
    public int? PayrollPeriodId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string? Format { get; set; } = "pdf"; // pdf, excel, csv
}

#endregion
