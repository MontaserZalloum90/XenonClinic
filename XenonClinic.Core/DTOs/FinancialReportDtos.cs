namespace XenonClinic.Core.DTOs;

#region Balance Sheet DTOs

/// <summary>
/// Balance Sheet report DTO
/// </summary>
public class BalanceSheetReportDto
{
    public DateTime ReportDate { get; set; }
    public string ReportPeriod { get; set; } = string.Empty;
    public string Currency { get; set; } = "AED";

    // Assets
    public BalanceSheetSectionDto CurrentAssets { get; set; } = new();
    public BalanceSheetSectionDto FixedAssets { get; set; } = new();
    public BalanceSheetSectionDto OtherAssets { get; set; } = new();
    public decimal TotalAssets { get; set; }

    // Liabilities
    public BalanceSheetSectionDto CurrentLiabilities { get; set; } = new();
    public BalanceSheetSectionDto LongTermLiabilities { get; set; } = new();
    public decimal TotalLiabilities { get; set; }

    // Equity
    public BalanceSheetSectionDto Equity { get; set; } = new();
    public decimal TotalEquity { get; set; }

    // Validation
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Section in a balance sheet
/// </summary>
public class BalanceSheetSectionDto
{
    public string SectionName { get; set; } = string.Empty;
    public List<BalanceSheetLineItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

/// <summary>
/// Line item in a balance sheet section
/// </summary>
public class BalanceSheetLineItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal? PriorPeriodBalance { get; set; }
    public decimal? ChangeAmount { get; set; }
    public decimal? ChangePercent { get; set; }
    public List<BalanceSheetLineItemDto>? SubItems { get; set; }
}

#endregion

#region Cash Flow Statement DTOs

/// <summary>
/// Cash Flow Statement report DTO
/// </summary>
public class CashFlowStatementDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "AED";

    // Opening Balance
    public decimal OpeningCashBalance { get; set; }

    // Operating Activities
    public CashFlowSectionDto OperatingActivities { get; set; } = new();
    public decimal NetCashFromOperations { get; set; }

    // Investing Activities
    public CashFlowSectionDto InvestingActivities { get; set; } = new();
    public decimal NetCashFromInvesting { get; set; }

    // Financing Activities
    public CashFlowSectionDto FinancingActivities { get; set; } = new();
    public decimal NetCashFromFinancing { get; set; }

    // Summary
    public decimal NetChangeInCash { get; set; }
    public decimal ClosingCashBalance { get; set; }

    // Reconciliation
    public decimal NetIncome { get; set; }
    public List<CashFlowAdjustmentDto> Adjustments { get; set; } = new();

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Section in cash flow statement
/// </summary>
public class CashFlowSectionDto
{
    public string SectionName { get; set; } = string.Empty;
    public List<CashFlowLineItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

/// <summary>
/// Line item in cash flow section
/// </summary>
public class CashFlowLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsInflow { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Adjustment item for cash flow reconciliation
/// </summary>
public class CashFlowAdjustmentDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AdjustmentType { get; set; } = string.Empty;
}

#endregion

#region Trial Balance DTOs

/// <summary>
/// Trial Balance report DTO
/// </summary>
public class TrialBalanceReportDto
{
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = "AED";

    public List<TrialBalanceLineItemDto> Items { get; set; } = new();

    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
    public decimal Difference => TotalDebits - TotalCredits;

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Line item in trial balance
/// </summary>
public class TrialBalanceLineItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
    public decimal NetBalance { get; set; }
}

#endregion

#region General Ledger DTOs

/// <summary>
/// General Ledger report DTO
/// </summary>
public class GeneralLedgerReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "AED";

    public List<GeneralLedgerAccountDto> Accounts { get; set; } = new();

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Account section in general ledger
/// </summary>
public class GeneralLedgerAccountDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }
    public List<GeneralLedgerEntryDto> Entries { get; set; } = new();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal ClosingBalance { get; set; }
}

/// <summary>
/// Entry in general ledger
/// </summary>
public class GeneralLedgerEntryDto
{
    public DateTime TransactionDate { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

#endregion

#region Accounts Receivable Aging DTOs

/// <summary>
/// Accounts Receivable Aging report DTO
/// </summary>
public class ARAgingReportDto
{
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = "AED";

    public List<ARAgingCustomerDto> Customers { get; set; } = new();

    // Summary
    public decimal TotalCurrent { get; set; }
    public decimal Total1To30Days { get; set; }
    public decimal Total31To60Days { get; set; }
    public decimal Total61To90Days { get; set; }
    public decimal TotalOver90Days { get; set; }
    public decimal GrandTotal { get; set; }

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Customer in AR aging report
/// </summary>
public class ARAgingCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }

    public decimal Current { get; set; }           // 0 days
    public decimal Days1To30 { get; set; }         // 1-30 days
    public decimal Days31To60 { get; set; }        // 31-60 days
    public decimal Days61To90 { get; set; }        // 61-90 days
    public decimal Over90Days { get; set; }        // 90+ days
    public decimal Total { get; set; }

    public List<ARAgingInvoiceDto> Invoices { get; set; } = new();
}

/// <summary>
/// Invoice in AR aging report
/// </summary>
public class ARAgingInvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int DaysOutstanding { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string AgingBucket { get; set; } = string.Empty;
}

#endregion

#region Accounts Payable Aging DTOs

/// <summary>
/// Accounts Payable Aging report DTO
/// </summary>
public class APAgingReportDto
{
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = "AED";

    public List<APAgingVendorDto> Vendors { get; set; } = new();

    // Summary
    public decimal TotalCurrent { get; set; }
    public decimal Total1To30Days { get; set; }
    public decimal Total31To60Days { get; set; }
    public decimal Total61To90Days { get; set; }
    public decimal TotalOver90Days { get; set; }
    public decimal GrandTotal { get; set; }

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Vendor in AP aging report
/// </summary>
public class APAgingVendorDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorCode { get; set; }

    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
    public decimal Total { get; set; }

    public List<APAgingBillDto> Bills { get; set; } = new();
}

/// <summary>
/// Bill in AP aging report
/// </summary>
public class APAgingBillDto
{
    public int BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int DaysOutstanding { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string AgingBucket { get; set; } = string.Empty;
}

#endregion

#region Enhanced P&L Report DTOs

/// <summary>
/// Enhanced Profit & Loss Statement DTO
/// </summary>
public class EnhancedProfitLossReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "AED";

    // Revenue Section
    public ProfitLossSectionDto Revenue { get; set; } = new();
    public decimal TotalRevenue { get; set; }

    // Cost of Services/Goods Sold
    public ProfitLossSectionDto CostOfServices { get; set; } = new();
    public decimal TotalCostOfServices { get; set; }

    // Gross Profit
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }

    // Operating Expenses
    public ProfitLossSectionDto OperatingExpenses { get; set; } = new();
    public decimal TotalOperatingExpenses { get; set; }

    // Operating Income
    public decimal OperatingIncome { get; set; }
    public decimal OperatingMargin { get; set; }

    // Other Income/Expenses
    public ProfitLossSectionDto OtherIncome { get; set; } = new();
    public ProfitLossSectionDto OtherExpenses { get; set; } = new();
    public decimal NetOtherIncomeExpense { get; set; }

    // Net Income Before Tax
    public decimal IncomeBeforeTax { get; set; }

    // Taxes
    public decimal TaxExpense { get; set; }

    // Net Income
    public decimal NetIncome { get; set; }
    public decimal NetProfitMargin { get; set; }

    // Comparative Data
    public EnhancedProfitLossReportDto? PriorPeriod { get; set; }
    public EnhancedProfitLossReportDto? Budget { get; set; }

    // Variance Analysis
    public List<VarianceItemDto> Variances { get; set; } = new();

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}

/// <summary>
/// Section in P&L report
/// </summary>
public class ProfitLossSectionDto
{
    public string SectionName { get; set; } = string.Empty;
    public List<ProfitLossLineItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

/// <summary>
/// Line item in P&L section
/// </summary>
public class ProfitLossLineItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public decimal? PriorPeriodAmount { get; set; }
    public decimal? BudgetAmount { get; set; }
    public decimal? VarianceAmount { get; set; }
    public decimal? VariancePercent { get; set; }
    public decimal PercentOfRevenue { get; set; }
}

/// <summary>
/// Variance item for analysis
/// </summary>
public class VarianceItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public decimal ActualAmount { get; set; }
    public decimal ComparisonAmount { get; set; }
    public decimal VarianceAmount { get; set; }
    public decimal VariancePercent { get; set; }
    public bool IsFavorable { get; set; }
    public string VarianceType { get; set; } = string.Empty; // "PriorPeriod" or "Budget"
}

#endregion

#region Financial Ratios DTOs

/// <summary>
/// Financial Ratios analysis DTO
/// </summary>
public class FinancialRatiosDto
{
    public DateTime AsOfDate { get; set; }

    // Liquidity Ratios
    public decimal CurrentRatio { get; set; }
    public decimal QuickRatio { get; set; }
    public decimal CashRatio { get; set; }
    public decimal WorkingCapital { get; set; }

    // Profitability Ratios
    public decimal GrossProfitMargin { get; set; }
    public decimal OperatingProfitMargin { get; set; }
    public decimal NetProfitMargin { get; set; }
    public decimal ReturnOnAssets { get; set; }
    public decimal ReturnOnEquity { get; set; }

    // Efficiency Ratios
    public decimal AssetTurnover { get; set; }
    public decimal ReceivablesTurnover { get; set; }
    public decimal DaysSalesOutstanding { get; set; }
    public decimal PayablesTurnover { get; set; }
    public decimal DaysPayablesOutstanding { get; set; }
    public decimal InventoryTurnover { get; set; }
    public decimal DaysInventoryOutstanding { get; set; }

    // Leverage Ratios
    public decimal DebtToEquity { get; set; }
    public decimal DebtToAssets { get; set; }
    public decimal InterestCoverageRatio { get; set; }

    // Healthcare-Specific Ratios
    public decimal RevenuePerPatient { get; set; }
    public decimal CostPerPatient { get; set; }
    public decimal PatientVisitsGrowthRate { get; set; }
    public decimal CollectionRate { get; set; }

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Report Request DTOs

/// <summary>
/// Request parameters for financial reports
/// </summary>
public class FinancialReportRequestDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludePriorPeriod { get; set; }
    public bool IncludeBudget { get; set; }
    public bool IncludeDetails { get; set; } = true;
    public string? AccountFilter { get; set; }
    public List<int>? BranchIds { get; set; }
    public string Currency { get; set; } = "AED";
    public string OutputFormat { get; set; } = "json"; // json, pdf, excel
}

/// <summary>
/// Request parameters for aging reports
/// </summary>
public class AgingReportRequestDto
{
    public DateTime AsOfDate { get; set; }
    public bool IncludeDetails { get; set; } = true;
    public List<int>? CustomerIds { get; set; }
    public List<int>? VendorIds { get; set; }
    public int[] AgingBuckets { get; set; } = { 0, 30, 60, 90 };
    public string Currency { get; set; } = "AED";
    public string OutputFormat { get; set; } = "json";
}

#endregion

#region Additional DTOs for Service Compatibility

/// <summary>
/// Income Statement DTO
/// </summary>
public class IncomeStatementDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Currency { get; set; } = "AED";
    public List<IncomeStatementLineDto>? RevenueItems { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<IncomeStatementLineDto>? ExpenseItems { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Income Statement Line Item DTO
/// </summary>
public class IncomeStatementLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Balance Sheet DTO (alias for BalanceSheetReportDto)
/// </summary>
public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = "AED";
    public List<BalanceSheetLineDto>? CurrentAssets { get; set; }
    public decimal TotalCurrentAssets { get; set; }
    public List<BalanceSheetLineDto>? NonCurrentAssets { get; set; }
    public decimal TotalNonCurrentAssets { get; set; }
    public decimal TotalAssets { get; set; }
    public List<BalanceSheetLineDto>? CurrentLiabilities { get; set; }
    public decimal TotalCurrentLiabilities { get; set; }
    public List<BalanceSheetLineDto>? NonCurrentLiabilities { get; set; }
    public decimal TotalNonCurrentLiabilities { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Balance Sheet Line DTO (alias for BalanceSheetLineItemDto)
/// </summary>
public class BalanceSheetLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal? PriorPeriodBalance { get; set; }
}

/// <summary>
/// Cash Flow Line DTO (alias for CashFlowLineItemDto)
/// </summary>
public class CashFlowLineDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsInflow { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Revenue Report DTO
/// </summary>
public class RevenueReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Currency { get; set; } = "AED";
    public decimal TotalRevenue { get; set; }
    public List<RevenueLineDto> RevenueLines { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Revenue Line DTO
/// </summary>
public class RevenueLineDto
{
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

#endregion
