using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Financial Report generation
/// </summary>
public interface IFinancialReportService
{
    #region Balance Sheet

    /// <summary>
    /// Generate Balance Sheet report
    /// </summary>
    Task<BalanceSheetReportDto> GenerateBalanceSheetAsync(int branchId, DateTime asOfDate);

    /// <summary>
    /// Generate comparative Balance Sheet (with prior period)
    /// </summary>
    Task<BalanceSheetReportDto> GenerateComparativeBalanceSheetAsync(int branchId, DateTime currentDate, DateTime priorDate);

    #endregion

    #region Profit & Loss

    /// <summary>
    /// Generate Profit & Loss report
    /// </summary>
    Task<EnhancedProfitLossReportDto> GenerateProfitLossAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generate comparative Profit & Loss (with prior period)
    /// </summary>
    Task<EnhancedProfitLossReportDto> GenerateComparativeProfitLossAsync(int branchId, FinancialReportRequestDto request);

    /// <summary>
    /// Generate Profit & Loss by department/cost center
    /// </summary>
    Task<Dictionary<string, EnhancedProfitLossReportDto>> GenerateProfitLossByDepartmentAsync(int branchId, DateTime startDate, DateTime endDate);

    #endregion

    #region Cash Flow

    /// <summary>
    /// Generate Cash Flow Statement
    /// </summary>
    Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Generate Cash Flow forecast
    /// </summary>
    Task<CashFlowStatementDto> GenerateCashFlowForecastAsync(int branchId, int forecastMonths);

    #endregion

    #region Trial Balance

    /// <summary>
    /// Generate Trial Balance
    /// </summary>
    Task<TrialBalanceReportDto> GenerateTrialBalanceAsync(int branchId, DateTime asOfDate);

    /// <summary>
    /// Generate Adjusted Trial Balance
    /// </summary>
    Task<TrialBalanceReportDto> GenerateAdjustedTrialBalanceAsync(int branchId, DateTime asOfDate, List<AdjustmentEntryDto>? adjustments = null);

    #endregion

    #region General Ledger

    /// <summary>
    /// Generate General Ledger report
    /// </summary>
    Task<GeneralLedgerReportDto> GenerateGeneralLedgerAsync(int branchId, DateTime startDate, DateTime endDate, int? accountId = null);

    /// <summary>
    /// Generate General Ledger for specific accounts
    /// </summary>
    Task<GeneralLedgerReportDto> GenerateGeneralLedgerForAccountsAsync(int branchId, DateTime startDate, DateTime endDate, List<int> accountIds);

    #endregion

    #region Aging Reports

    /// <summary>
    /// Generate Accounts Receivable Aging report
    /// </summary>
    Task<ARAgingReportDto> GenerateARAgingReportAsync(int branchId, AgingReportRequestDto request);

    /// <summary>
    /// Generate Accounts Payable Aging report
    /// </summary>
    Task<APAgingReportDto> GenerateAPAgingReportAsync(int branchId, AgingReportRequestDto request);

    #endregion

    #region Financial Ratios

    /// <summary>
    /// Calculate financial ratios
    /// </summary>
    Task<FinancialRatiosDto> CalculateFinancialRatiosAsync(int branchId, DateTime asOfDate);

    #endregion

    #region Export

    /// <summary>
    /// Export report to PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync<T>(T report, string reportType) where T : class;

    /// <summary>
    /// Export report to Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync<T>(T report, string reportType) where T : class;

    /// <summary>
    /// Export report to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync<T>(T report, string reportType) where T : class;

    #endregion
}

/// <summary>
/// DTO for adjustment entry
/// </summary>
public class AdjustmentEntryDto
{
    public int AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
}
