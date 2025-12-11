using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Payroll operations
/// </summary>
public interface IPayrollService
{
    #region Payroll Period Management

    /// <summary>
    /// Get all payroll periods for a branch
    /// </summary>
    Task<IEnumerable<PayrollPeriodDto>> GetPayrollPeriodsAsync(int branchId, int? year = null);

    /// <summary>
    /// Get a payroll period by ID
    /// </summary>
    Task<PayrollPeriodDto?> GetPayrollPeriodByIdAsync(int id);

    /// <summary>
    /// Get the current/active payroll period
    /// </summary>
    Task<PayrollPeriodDto?> GetCurrentPayrollPeriodAsync(int branchId);

    /// <summary>
    /// Create a new payroll period
    /// </summary>
    Task<PayrollPeriodDto> CreatePayrollPeriodAsync(int branchId, CreatePayrollPeriodDto dto);

    /// <summary>
    /// Close a payroll period
    /// </summary>
    Task<PayrollPeriodDto> ClosePayrollPeriodAsync(int periodId);

    #endregion

    #region Payroll Processing

    /// <summary>
    /// Process payroll for a period
    /// </summary>
    Task<PayrollPeriodDto> ProcessPayrollAsync(ProcessPayrollRequestDto request);

    /// <summary>
    /// Recalculate a single employee's payroll
    /// </summary>
    Task<PayslipDto> RecalculateEmployeePayrollAsync(int periodId, int employeeId);

    /// <summary>
    /// Approve payroll for a period
    /// </summary>
    Task<PayrollPeriodDto> ApprovePayrollAsync(ApprovePayrollRequestDto request);

    /// <summary>
    /// Mark payroll as paid
    /// </summary>
    Task<PayrollPeriodDto> MarkPayrollPaidAsync(int periodId, DateTime paymentDate);

    #endregion

    #region Payslips

    /// <summary>
    /// Get payslips for a payroll period
    /// </summary>
    Task<IEnumerable<PayslipSummaryDto>> GetPayslipsForPeriodAsync(int periodId, int? departmentId = null);

    /// <summary>
    /// Get a specific payslip
    /// </summary>
    Task<PayslipDto?> GetPayslipByIdAsync(int id);

    /// <summary>
    /// Get payslips for an employee
    /// </summary>
    Task<IEnumerable<PayslipSummaryDto>> GetEmployeePayslipsAsync(int employeeId, int? year = null);

    /// <summary>
    /// Get the latest payslip for an employee
    /// </summary>
    Task<PayslipDto?> GetLatestPayslipAsync(int employeeId);

    /// <summary>
    /// Generate payslip PDF
    /// </summary>
    Task<byte[]> GeneratePayslipPdfAsync(int payslipId);

    /// <summary>
    /// Email payslip to employee
    /// </summary>
    Task<bool> EmailPayslipAsync(int payslipId, string? emailOverride = null);

    /// <summary>
    /// Bulk email payslips for a period
    /// </summary>
    Task<int> BulkEmailPayslipsAsync(int periodId);

    #endregion

    #region WPS (Wage Protection System)

    /// <summary>
    /// Generate WPS file
    /// </summary>
    Task<WpsFileResponseDto> GenerateWpsFileAsync(WpsGenerationRequestDto request);

    /// <summary>
    /// Validate WPS data before generation
    /// </summary>
    Task<WpsFileResponseDto> ValidateWpsDataAsync(int periodId);

    /// <summary>
    /// Get WPS submission history
    /// </summary>
    Task<IEnumerable<WpsSubmissionDto>> GetWpsSubmissionsAsync(int branchId, int? year = null);

    /// <summary>
    /// Record WPS submission
    /// </summary>
    Task<WpsSubmissionDto> RecordWpsSubmissionAsync(int periodId, string fileName, string referenceNumber);

    /// <summary>
    /// Update WPS submission status
    /// </summary>
    Task<WpsSubmissionDto> UpdateWpsSubmissionStatusAsync(int submissionId, string status, string? rejectionReason = null);

    #endregion

    #region Reports

    /// <summary>
    /// Generate payroll summary report
    /// </summary>
    Task<PayrollSummaryReportDto> GeneratePayrollSummaryReportAsync(int periodId);

    /// <summary>
    /// Generate payroll register report
    /// </summary>
    Task<PayrollRegisterReportDto> GeneratePayrollRegisterReportAsync(int periodId, int? departmentId = null);

    /// <summary>
    /// Generate bank transfer list
    /// </summary>
    Task<BankTransferListReportDto> GenerateBankTransferListAsync(int periodId);

    /// <summary>
    /// Generate year-to-date earnings report for an employee
    /// </summary>
    Task<YtdEarningsReportDto> GenerateYtdEarningsReportAsync(int employeeId, int year);

    /// <summary>
    /// Generate employee cost report
    /// </summary>
    Task<EmployeeCostReportDto> GenerateEmployeeCostReportAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generate payroll variance report
    /// </summary>
    Task<PayrollVarianceReportDto> GenerateVarianceReportAsync(int currentPeriodId, int previousPeriodId);

    /// <summary>
    /// Export report to specified format
    /// </summary>
    Task<byte[]> ExportReportAsync(PayrollReportRequestDto request);

    #endregion

    #region Configuration

    /// <summary>
    /// Get salary components
    /// </summary>
    Task<IEnumerable<SalaryComponentDto>> GetSalaryComponentsAsync(int branchId);

    /// <summary>
    /// Create or update salary component
    /// </summary>
    Task<SalaryComponentDto> SaveSalaryComponentAsync(int branchId, SalaryComponentDto dto);

    /// <summary>
    /// Get tax configurations
    /// </summary>
    Task<IEnumerable<TaxConfigurationDto>> GetTaxConfigurationsAsync(int branchId);

    /// <summary>
    /// Create or update tax configuration
    /// </summary>
    Task<TaxConfigurationDto> SaveTaxConfigurationAsync(int branchId, TaxConfigurationDto dto);

    #endregion

    #region Utility

    /// <summary>
    /// Generate payroll period code
    /// </summary>
    Task<string> GeneratePeriodCodeAsync(int branchId, DateTime periodStart);

    /// <summary>
    /// Calculate gross pay for an employee
    /// </summary>
    Task<decimal> CalculateGrossPayAsync(int employeeId, int periodId);

    /// <summary>
    /// Calculate deductions for an employee
    /// </summary>
    Task<decimal> CalculateDeductionsAsync(int employeeId, int periodId, decimal grossPay);

    #endregion
}
