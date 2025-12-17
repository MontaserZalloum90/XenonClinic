using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a payroll processing period
/// </summary>
public class PayrollPeriod : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Processing, Approved, Paid, Closed
    public string? Notes { get; set; }

    public DateTime? ProcessedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<Payslip>? Payslips { get; set; }
    public ICollection<WpsSubmission>? WpsSubmissions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents an employee payslip
/// </summary>
public class Payslip
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    // Earnings
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal FoodAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal Overtime { get; set; }
    public decimal Bonus { get; set; }
    public decimal Commission { get; set; }
    public decimal GrossPay { get; set; }

    // Deductions
    public decimal SocialInsurance { get; set; }
    public decimal HealthInsurance { get; set; }
    public decimal Tax { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }

    // Net Pay
    public decimal NetPay { get; set; }

    // Work info
    public int DaysWorked { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDaysTaken { get; set; }

    public string Status { get; set; } = "Calculated"; // Calculated, Approved, Paid
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public object GrossSalary { get; set; }
    public object NetSalary { get; set; }
    public object Deductions { get; set; }
}

/// <summary>
/// Represents a WPS (Wage Protection System) file submission
/// </summary>
public class WpsSubmission
{
    public int Id { get; set; }
    public int PayrollPeriodId { get; set; }
    public PayrollPeriod? PayrollPeriod { get; set; }

    public string FileName { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Submitted, Accepted, Rejected, Partially Processed
    public string? ReferenceNumber { get; set; }
    public int? ProcessedRecords { get; set; }
    public int? RejectedRecords { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents a salary component configuration
/// </summary>
public class SalaryComponent : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string ComponentType { get; set; } = string.Empty; // Earning, Deduction
    public string Category { get; set; } = string.Empty; // Basic, Allowance, Bonus, Tax, Insurance
    public bool IsTaxable { get; set; }
    public bool IsPartOfGross { get; set; }
    public string CalculationType { get; set; } = "Fixed"; // Fixed, Percentage, Formula
    public decimal? DefaultValue { get; set; }
    public string? Formula { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents tax configuration
/// </summary>
public class TaxConfiguration : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string TaxType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
