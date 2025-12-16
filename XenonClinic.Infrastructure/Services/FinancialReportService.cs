using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Financial Report generation
/// </summary>
public class FinancialReportService : IFinancialReportService
{
    private readonly ClinicDbContext _context;
    private readonly IExportService _exportService;

    public FinancialReportService(ClinicDbContext context, IExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }

    #region Balance Sheet

    public async Task<BalanceSheetReportDto> GenerateBalanceSheetAsync(int branchId, DateTime asOfDate)
    {
        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.IsActive)
            .ToListAsync();

        var report = new BalanceSheetReportDto
        {
            ReportDate = asOfDate,
            ReportPeriod = asOfDate.ToString("MMMM yyyy"),
            GeneratedAt = DateTime.UtcNow
        };

        // Current Assets
        var currentAssets = accounts.Where(a => a.AccountType == AccountType.Asset && IsCurrentAsset(a.AccountCode)).ToList();
        report.CurrentAssets = new BalanceSheetSectionDto
        {
            SectionName = "Current Assets",
            Items = currentAssets.Select(a => new BalanceSheetLineItemDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = a.Balance
            }).ToList(),
            Total = currentAssets.Sum(a => a.Balance)
        };

        // Fixed Assets
        var fixedAssets = accounts.Where(a => a.AccountType == AccountType.Asset && !IsCurrentAsset(a.AccountCode)).ToList();
        report.FixedAssets = new BalanceSheetSectionDto
        {
            SectionName = "Fixed Assets",
            Items = fixedAssets.Select(a => new BalanceSheetLineItemDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = a.Balance
            }).ToList(),
            Total = fixedAssets.Sum(a => a.Balance)
        };

        report.TotalAssets = report.CurrentAssets.Total + report.FixedAssets.Total + report.OtherAssets.Total;

        // Current Liabilities
        var currentLiabilities = accounts.Where(a => a.AccountType == AccountType.Liability && IsCurrentLiability(a.AccountCode)).ToList();
        report.CurrentLiabilities = new BalanceSheetSectionDto
        {
            SectionName = "Current Liabilities",
            Items = currentLiabilities.Select(a => new BalanceSheetLineItemDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = a.Balance
            }).ToList(),
            Total = currentLiabilities.Sum(a => a.Balance)
        };

        // Long-term Liabilities
        var longTermLiabilities = accounts.Where(a => a.AccountType == AccountType.Liability && !IsCurrentLiability(a.AccountCode)).ToList();
        report.LongTermLiabilities = new BalanceSheetSectionDto
        {
            SectionName = "Long-term Liabilities",
            Items = longTermLiabilities.Select(a => new BalanceSheetLineItemDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = a.Balance
            }).ToList(),
            Total = longTermLiabilities.Sum(a => a.Balance)
        };

        report.TotalLiabilities = report.CurrentLiabilities.Total + report.LongTermLiabilities.Total;

        // Equity
        var equity = accounts.Where(a => a.AccountType == AccountType.Equity).ToList();
        report.Equity = new BalanceSheetSectionDto
        {
            SectionName = "Equity",
            Items = equity.Select(a => new BalanceSheetLineItemDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = a.Balance
            }).ToList(),
            Total = equity.Sum(a => a.Balance)
        };

        report.TotalEquity = report.Equity.Total;

        return report;
    }

    public async Task<BalanceSheetReportDto> GenerateComparativeBalanceSheetAsync(int branchId, DateTime currentDate, DateTime priorDate)
    {
        var currentReport = await GenerateBalanceSheetAsync(branchId, currentDate);
        var priorReport = await GenerateBalanceSheetAsync(branchId, priorDate);

        // Add variance calculations
        AddVarianceToSection(currentReport.CurrentAssets, priorReport.CurrentAssets);
        AddVarianceToSection(currentReport.FixedAssets, priorReport.FixedAssets);
        AddVarianceToSection(currentReport.CurrentLiabilities, priorReport.CurrentLiabilities);
        AddVarianceToSection(currentReport.LongTermLiabilities, priorReport.LongTermLiabilities);
        AddVarianceToSection(currentReport.Equity, priorReport.Equity);

        return currentReport;
    }

    #endregion

    #region Profit & Loss

    public async Task<EnhancedProfitLossReportDto> GenerateProfitLossAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.Account!.BranchId == branchId &&
                   t.TransactionDate >= startDate &&
                   t.TransactionDate <= endDate &&
                   t.Status != VoucherStatus.Voided)
            .ToListAsync();

        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.IsActive)
            .ToListAsync();

        var report = new EnhancedProfitLossReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Revenue Section
        var revenueAccounts = accounts.Where(a => a.AccountType == AccountType.Revenue).ToList();
        report.Revenue = new ProfitLossSectionDto
        {
            SectionName = "Revenue",
            Items = revenueAccounts.Select(a =>
            {
                var accountTransactions = transactions.Where(t => t.AccountId == a.Id).ToList();
                var amount = accountTransactions.Sum(t => t.TransactionType == TransactionType.Credit ? t.Amount : -t.Amount);
                return new ProfitLossLineItemDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Amount = amount
                };
            }).Where(i => i.Amount != 0).ToList()
        };
        report.Revenue.Total = report.Revenue.Items.Sum(i => i.Amount);
        report.TotalRevenue = report.Revenue.Total;

        // Cost of Services
        var cosAccounts = accounts.Where(a => a.AccountType == AccountType.Expense && IsCostOfService(a.AccountCode)).ToList();
        report.CostOfServices = new ProfitLossSectionDto
        {
            SectionName = "Cost of Services",
            Items = cosAccounts.Select(a =>
            {
                var accountTransactions = transactions.Where(t => t.AccountId == a.Id).ToList();
                var amount = accountTransactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount);
                return new ProfitLossLineItemDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Amount = amount
                };
            }).Where(i => i.Amount != 0).ToList()
        };
        report.CostOfServices.Total = report.CostOfServices.Items.Sum(i => i.Amount);
        report.TotalCostOfServices = report.CostOfServices.Total;

        // Gross Profit
        report.GrossProfit = report.TotalRevenue - report.TotalCostOfServices;
        report.GrossProfitMargin = report.TotalRevenue > 0 ? (report.GrossProfit / report.TotalRevenue) * 100 : 0;

        // Operating Expenses
        var opexAccounts = accounts.Where(a => a.AccountType == AccountType.Expense && !IsCostOfService(a.AccountCode)).ToList();
        report.OperatingExpenses = new ProfitLossSectionDto
        {
            SectionName = "Operating Expenses",
            Items = opexAccounts.Select(a =>
            {
                var accountTransactions = transactions.Where(t => t.AccountId == a.Id).ToList();
                var amount = accountTransactions.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount);
                return new ProfitLossLineItemDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Amount = amount
                };
            }).Where(i => i.Amount != 0).ToList()
        };
        report.OperatingExpenses.Total = report.OperatingExpenses.Items.Sum(i => i.Amount);
        report.TotalOperatingExpenses = report.OperatingExpenses.Total;

        // Operating Income
        report.OperatingIncome = report.GrossProfit - report.TotalOperatingExpenses;
        report.OperatingMargin = report.TotalRevenue > 0 ? (report.OperatingIncome / report.TotalRevenue) * 100 : 0;

        // Net Income
        report.IncomeBeforeTax = report.OperatingIncome + report.NetOtherIncomeExpense;
        report.NetIncome = report.IncomeBeforeTax - report.TaxExpense;
        report.NetProfitMargin = report.TotalRevenue > 0 ? (report.NetIncome / report.TotalRevenue) * 100 : 0;

        // Calculate percentage of revenue for each line item
        foreach (var item in report.Revenue.Items)
        {
            item.PercentOfRevenue = report.TotalRevenue > 0 ? (item.Amount / report.TotalRevenue) * 100 : 0;
        }

        foreach (var item in report.OperatingExpenses.Items)
        {
            item.PercentOfRevenue = report.TotalRevenue > 0 ? (item.Amount / report.TotalRevenue) * 100 : 0;
        }

        return report;
    }

    public async Task<EnhancedProfitLossReportDto> GenerateComparativeProfitLossAsync(int branchId, FinancialReportRequestDto request)
    {
        var currentReport = await GenerateProfitLossAsync(branchId, request.StartDate, request.EndDate);

        if (request.IncludePriorPeriod)
        {
            var periodLength = (request.EndDate - request.StartDate).Days;
            var priorStartDate = request.StartDate.AddDays(-periodLength - 1);
            var priorEndDate = request.StartDate.AddDays(-1);
            currentReport.PriorPeriod = await GenerateProfitLossAsync(branchId, priorStartDate, priorEndDate);

            // Calculate variances
            currentReport.Variances = CalculateVariances(currentReport, currentReport.PriorPeriod);
        }

        return currentReport;
    }

    public async Task<Dictionary<string, EnhancedProfitLossReportDto>> GenerateProfitLossByDepartmentAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.BranchId == branchId)
            .ToListAsync();

        var result = new Dictionary<string, EnhancedProfitLossReportDto>();

        // Generate overall report
        result["Overall"] = await GenerateProfitLossAsync(branchId, startDate, endDate);

        // In a real implementation, transactions would be tagged by department
        foreach (var dept in departments)
        {
            result[dept.Name] = await GenerateProfitLossAsync(branchId, startDate, endDate);
        }

        return result;
    }

    #endregion

    #region Cash Flow

    public async Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.Account!.BranchId == branchId &&
                   t.TransactionDate >= startDate &&
                   t.TransactionDate <= endDate &&
                   t.Status != VoucherStatus.Voided)
            .ToListAsync();

        var cashAccounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Asset &&
                   (a.AccountCode.StartsWith("1001") || a.AccountName.Contains("Cash") || a.AccountName.Contains("Bank")))
            .ToListAsync();

        var report = new CashFlowStatementDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Opening cash balance - would need historical data
        report.OpeningCashBalance = cashAccounts.Sum(a => a.Balance);

        // Operating Activities
        report.OperatingActivities = new CashFlowSectionDto
        {
            SectionName = "Operating Activities",
            Items = new List<CashFlowLineItemDto>
            {
                new() { Description = "Collections from Patients", Amount = await GetCollectionsFromPatientsAsync(branchId, startDate, endDate), IsInflow = true },
                new() { Description = "Payments to Suppliers", Amount = await GetPaymentsToSuppliersAsync(branchId, startDate, endDate), IsInflow = false },
                new() { Description = "Payments to Employees", Amount = await GetPaymentsToEmployeesAsync(branchId, startDate, endDate), IsInflow = false },
                new() { Description = "Other Operating Expenses", Amount = await GetOtherOperatingExpensesAsync(branchId, startDate, endDate), IsInflow = false }
            }
        };
        report.OperatingActivities.Total = report.OperatingActivities.Items.Sum(i => i.IsInflow ? i.Amount : -i.Amount);
        report.NetCashFromOperations = report.OperatingActivities.Total;

        // Investing Activities
        report.InvestingActivities = new CashFlowSectionDto
        {
            SectionName = "Investing Activities",
            Items = new List<CashFlowLineItemDto>
            {
                new() { Description = "Purchase of Equipment", Amount = 0, IsInflow = false },
                new() { Description = "Sale of Equipment", Amount = 0, IsInflow = true }
            }
        };
        report.InvestingActivities.Total = report.InvestingActivities.Items.Sum(i => i.IsInflow ? i.Amount : -i.Amount);
        report.NetCashFromInvesting = report.InvestingActivities.Total;

        // Financing Activities
        report.FinancingActivities = new CashFlowSectionDto
        {
            SectionName = "Financing Activities",
            Items = new List<CashFlowLineItemDto>
            {
                new() { Description = "Owner Contributions", Amount = 0, IsInflow = true },
                new() { Description = "Owner Drawings", Amount = 0, IsInflow = false },
                new() { Description = "Loan Proceeds", Amount = 0, IsInflow = true },
                new() { Description = "Loan Repayments", Amount = 0, IsInflow = false }
            }
        };
        report.FinancingActivities.Total = report.FinancingActivities.Items.Sum(i => i.IsInflow ? i.Amount : -i.Amount);
        report.NetCashFromFinancing = report.FinancingActivities.Total;

        // Summary
        report.NetChangeInCash = report.NetCashFromOperations + report.NetCashFromInvesting + report.NetCashFromFinancing;
        report.ClosingCashBalance = report.OpeningCashBalance + report.NetChangeInCash;

        return report;
    }

    public async Task<CashFlowStatementDto> GenerateCashFlowForecastAsync(int branchId, int forecastMonths)
    {
        // Generate forecast based on historical trends
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-12);

        var historicalCashFlow = await GenerateCashFlowStatementAsync(branchId, startDate, endDate);

        // Simple forecast - average monthly cash flow projected forward
        var monthlyNetCash = historicalCashFlow.NetChangeInCash / 12;

        var forecast = new CashFlowStatementDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(forecastMonths),
            GeneratedAt = DateTime.UtcNow,
            OpeningCashBalance = historicalCashFlow.ClosingCashBalance,
            NetChangeInCash = monthlyNetCash * forecastMonths,
            ClosingCashBalance = historicalCashFlow.ClosingCashBalance + (monthlyNetCash * forecastMonths)
        };

        return forecast;
    }

    #endregion

    #region Trial Balance

    public async Task<TrialBalanceReportDto> GenerateTrialBalanceAsync(int branchId, DateTime asOfDate)
    {
        var accounts = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        var report = new TrialBalanceReportDto
        {
            AsOfDate = asOfDate,
            GeneratedAt = DateTime.UtcNow,
            Items = accounts.Select(a =>
            {
                var isDebitBalance = a.AccountType == AccountType.Asset || a.AccountType == AccountType.Expense;
                return new TrialBalanceLineItemDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType.ToString(),
                    DebitBalance = isDebitBalance && a.Balance > 0 ? a.Balance : (isDebitBalance && a.Balance < 0 ? 0 : (a.Balance < 0 ? -a.Balance : 0)),
                    CreditBalance = !isDebitBalance && a.Balance > 0 ? a.Balance : (!isDebitBalance && a.Balance < 0 ? 0 : (a.Balance < 0 ? -a.Balance : 0)),
                    NetBalance = a.Balance
                };
            }).ToList()
        };

        report.TotalDebits = report.Items.Sum(i => i.DebitBalance);
        report.TotalCredits = report.Items.Sum(i => i.CreditBalance);

        return report;
    }

    public async Task<TrialBalanceReportDto> GenerateAdjustedTrialBalanceAsync(int branchId, DateTime asOfDate, List<AdjustmentEntryDto>? adjustments = null)
    {
        var report = await GenerateTrialBalanceAsync(branchId, asOfDate);

        if (adjustments != null && adjustments.Any())
        {
            foreach (var adjustment in adjustments)
            {
                var item = report.Items.FirstOrDefault(i => i.AccountCode == adjustment.AccountId.ToString());
                if (item != null)
                {
                    item.DebitBalance += adjustment.DebitAmount;
                    item.CreditBalance += adjustment.CreditAmount;
                    item.NetBalance = item.DebitBalance - item.CreditBalance;
                }
            }

            report.TotalDebits = report.Items.Sum(i => i.DebitBalance);
            report.TotalCredits = report.Items.Sum(i => i.CreditBalance);
        }

        return report;
    }

    #endregion

    #region General Ledger

    public async Task<GeneralLedgerReportDto> GenerateGeneralLedgerAsync(int branchId, DateTime startDate, DateTime endDate, int? accountId = null)
    {
        var accountsQuery = _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.IsActive);

        if (accountId.HasValue)
        {
            accountsQuery = accountsQuery.Where(a => a.Id == accountId.Value);
        }

        var accounts = await accountsQuery.OrderBy(a => a.AccountCode).ToListAsync();

        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.Account!.BranchId == branchId &&
                   t.TransactionDate >= startDate &&
                   t.TransactionDate <= endDate &&
                   t.Status != VoucherStatus.Voided)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync();

        var report = new GeneralLedgerReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            Accounts = accounts.Select(a =>
            {
                var accountTransactions = transactions.Where(t => t.AccountId == a.Id).ToList();
                var ledgerAccount = new GeneralLedgerAccountDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType.ToString(),
                    OpeningBalance = a.Balance - accountTransactions.Sum(t =>
                        t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount),
                    Entries = new List<GeneralLedgerEntryDto>()
                };

                decimal runningBalance = ledgerAccount.OpeningBalance;
                foreach (var t in accountTransactions)
                {
                    var isDebit = t.TransactionType == TransactionType.Debit;
                    runningBalance += isDebit ? t.Amount : -t.Amount;

                    ledgerAccount.Entries.Add(new GeneralLedgerEntryDto
                    {
                        TransactionDate = t.TransactionDate,
                        TransactionNumber = t.TransactionNumber,
                        Description = t.Description,
                        ReferenceNumber = t.ReferenceNumber,
                        DebitAmount = isDebit ? t.Amount : 0,
                        CreditAmount = !isDebit ? t.Amount : 0,
                        RunningBalance = runningBalance
                    });
                }

                ledgerAccount.TotalDebits = ledgerAccount.Entries.Sum(e => e.DebitAmount);
                ledgerAccount.TotalCredits = ledgerAccount.Entries.Sum(e => e.CreditAmount);
                ledgerAccount.ClosingBalance = runningBalance;

                return ledgerAccount;
            }).ToList()
        };

        return report;
    }

    public async Task<GeneralLedgerReportDto> GenerateGeneralLedgerForAccountsAsync(int branchId, DateTime startDate, DateTime endDate, List<int> accountIds)
    {
        var report = new GeneralLedgerReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow,
            Accounts = new List<GeneralLedgerAccountDto>()
        };

        foreach (var accountId in accountIds)
        {
            var accountReport = await GenerateGeneralLedgerAsync(branchId, startDate, endDate, accountId);
            report.Accounts.AddRange(accountReport.Accounts);
        }

        return report;
    }

    #endregion

    #region Aging Reports

    public async Task<ARAgingReportDto> GenerateARAgingReportAsync(int branchId, AgingReportRequestDto request)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Branch)
            .Where(i => i.BranchId == branchId &&
                   i.Status != InvoiceStatus.Paid &&
                   i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        var patients = await _context.Patients
            .AsNoTracking()
            .Where(p => p.BranchId == branchId)
            .ToListAsync();

        var report = new ARAgingReportDto
        {
            AsOfDate = request.AsOfDate,
            GeneratedAt = DateTime.UtcNow,
            Customers = new List<ARAgingCustomerDto>()
        };

        var invoicesByPatient = invoices.GroupBy(i => i.PatientId);

        foreach (var group in invoicesByPatient)
        {
            var patient = patients.FirstOrDefault(p => p.Id == group.Key);
            if (patient == null) continue;

            var customerAging = new ARAgingCustomerDto
            {
                CustomerId = patient.Id,
                CustomerName = $"{patient.FirstName} {patient.LastName}",
                Invoices = new List<ARAgingInvoiceDto>()
            };

            foreach (var invoice in group)
            {
                var daysOutstanding = (request.AsOfDate - invoice.InvoiceDate).Days;
                var outstandingAmount = invoice.TotalAmount - invoice.PaidAmount;
                var agingBucket = GetAgingBucket(daysOutstanding);

                customerAging.Invoices.Add(new ARAgingInvoiceDto
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    DaysOutstanding = daysOutstanding,
                    OriginalAmount = invoice.TotalAmount,
                    PaidAmount = invoice.PaidAmount,
                    OutstandingAmount = outstandingAmount,
                    AgingBucket = agingBucket
                });

                // Update bucket totals
                switch (agingBucket)
                {
                    case "Current":
                        customerAging.Current += outstandingAmount;
                        break;
                    case "1-30 Days":
                        customerAging.Days1To30 += outstandingAmount;
                        break;
                    case "31-60 Days":
                        customerAging.Days31To60 += outstandingAmount;
                        break;
                    case "61-90 Days":
                        customerAging.Days61To90 += outstandingAmount;
                        break;
                    default:
                        customerAging.Over90Days += outstandingAmount;
                        break;
                }
            }

            customerAging.Total = customerAging.Current + customerAging.Days1To30 +
                                 customerAging.Days31To60 + customerAging.Days61To90 +
                                 customerAging.Over90Days;

            report.Customers.Add(customerAging);
        }

        // Calculate totals
        report.TotalCurrent = report.Customers.Sum(c => c.Current);
        report.Total1To30Days = report.Customers.Sum(c => c.Days1To30);
        report.Total31To60Days = report.Customers.Sum(c => c.Days31To60);
        report.Total61To90Days = report.Customers.Sum(c => c.Days61To90);
        report.TotalOver90Days = report.Customers.Sum(c => c.Over90Days);
        report.GrandTotal = report.Customers.Sum(c => c.Total);

        return report;
    }

    public async Task<APAgingReportDto> GenerateAPAgingReportAsync(int branchId, AgingReportRequestDto request)
    {
        var purchaseOrders = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Supplier)
            .Where(po => po.BranchId == branchId &&
                   po.Status != PurchaseOrderStatus.Cancelled)
            .ToListAsync();

        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.BranchId == branchId)
            .ToListAsync();

        var report = new APAgingReportDto
        {
            AsOfDate = request.AsOfDate,
            GeneratedAt = DateTime.UtcNow,
            Vendors = new List<APAgingVendorDto>()
        };

        // Similar implementation to AR Aging...
        // For brevity, returning empty report structure
        return report;
    }

    #endregion

    #region Financial Ratios

    public async Task<FinancialRatiosDto> CalculateFinancialRatiosAsync(int branchId, DateTime asOfDate)
    {
        var balanceSheet = await GenerateBalanceSheetAsync(branchId, asOfDate);
        var startOfYear = new DateTime(asOfDate.Year, 1, 1);
        var profitLoss = await GenerateProfitLossAsync(branchId, startOfYear, asOfDate);

        var ratios = new FinancialRatiosDto
        {
            AsOfDate = asOfDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Liquidity Ratios
        var currentAssets = balanceSheet.CurrentAssets.Total;
        var currentLiabilities = balanceSheet.CurrentLiabilities.Total;

        ratios.CurrentRatio = currentLiabilities > 0 ? currentAssets / currentLiabilities : 0;
        ratios.QuickRatio = currentLiabilities > 0 ? (currentAssets * 0.8m) / currentLiabilities : 0; // Simplified
        ratios.WorkingCapital = currentAssets - currentLiabilities;

        // Profitability Ratios
        ratios.GrossProfitMargin = profitLoss.GrossProfitMargin;
        ratios.OperatingProfitMargin = profitLoss.OperatingMargin;
        ratios.NetProfitMargin = profitLoss.NetProfitMargin;

        var totalAssets = balanceSheet.TotalAssets;
        var totalEquity = balanceSheet.TotalEquity;

        ratios.ReturnOnAssets = totalAssets > 0 ? (profitLoss.NetIncome / totalAssets) * 100 : 0;
        ratios.ReturnOnEquity = totalEquity > 0 ? (profitLoss.NetIncome / totalEquity) * 100 : 0;

        // Efficiency Ratios
        ratios.AssetTurnover = totalAssets > 0 ? profitLoss.TotalRevenue / totalAssets : 0;

        // Healthcare-Specific
        var patientCount = await _context.Patients.CountAsync(p => p.BranchId == branchId);
        ratios.RevenuePerPatient = patientCount > 0 ? profitLoss.TotalRevenue / patientCount : 0;
        ratios.CostPerPatient = patientCount > 0 ? profitLoss.TotalOperatingExpenses / patientCount : 0;

        // Collection Rate
        var totalInvoiced = await _context.Invoices
            .Where(i => i.BranchId == branchId && i.InvoiceDate >= startOfYear && i.InvoiceDate <= asOfDate)
            .SumAsync(i => i.TotalAmount);

        var totalCollected = await _context.Invoices
            .Where(i => i.BranchId == branchId && i.InvoiceDate >= startOfYear && i.InvoiceDate <= asOfDate)
            .SumAsync(i => i.PaidAmount);

        ratios.CollectionRate = totalInvoiced > 0 ? (totalCollected / totalInvoiced) * 100 : 0;

        return ratios;
    }

    #endregion

    #region Export

    public async Task<byte[]> ExportToPdfAsync<T>(T report, string reportType) where T : class
    {
        await Task.CompletedTask;
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Column(header =>
                {
                    header.Item().Text($"FINANCIAL REPORT: {reportType}").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    header.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}").FontSize(9).FontColor(Colors.Grey.Medium);
                    header.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                });

                // Content - Dynamic based on report type
                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (report is IncomeStatementDto incomeStatement)
                    {
                        RenderIncomeStatement(col, incomeStatement);
                    }
                    else if (report is BalanceSheetDto balanceSheet)
                    {
                        RenderBalanceSheet(col, balanceSheet);
                    }
                    else if (report is CashFlowStatementDto cashFlow)
                    {
                        RenderCashFlowStatement(col, cashFlow);
                    }
                    else if (report is RevenueReportDto revenueReport)
                    {
                        RenderRevenueReport(col, revenueReport);
                    }
                    else
                    {
                        // Generic rendering for other report types
                        col.Item().Text("Report Data").Bold().FontSize(12);
                        col.Item().PaddingTop(10).Text(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }))
                            .FontSize(9);
                    }
                });

                // Footer
                page.Footer().Column(footer =>
                {
                    footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    footer.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("Confidential - For Internal Use Only").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(100).AlignRight().DefaultTextStyle(x => x.FontSize(8)).Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static void RenderIncomeStatement(ColumnDescriptor col, IncomeStatementDto report)
    {
        col.Item().Text($"Income Statement - {report.PeriodStart:MMM dd, yyyy} to {report.PeriodEnd:MMM dd, yyyy}").Bold().FontSize(14);

        // Revenue Section
        col.Item().PaddingTop(15).Text("REVENUE").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });
            foreach (var item in report.RevenueItems ?? Enumerable.Empty<IncomeStatementLineDto>())
            {
                table.Cell().Padding(3).Text(item.Description);
                table.Cell().Padding(3).AlignRight().Text($"{item.Amount:C}");
            }
            table.Cell().BorderTop(1).Padding(3).Text("Total Revenue").Bold();
            table.Cell().BorderTop(1).Padding(3).AlignRight().Text($"{report.TotalRevenue:C}").Bold().FontColor(Colors.Green.Darken2);
        });

        // Expenses Section
        col.Item().PaddingTop(15).Text("EXPENSES").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });
            foreach (var item in report.ExpenseItems ?? Enumerable.Empty<IncomeStatementLineDto>())
            {
                table.Cell().Padding(3).Text(item.Description);
                table.Cell().Padding(3).AlignRight().Text($"{item.Amount:C}");
            }
            table.Cell().BorderTop(1).Padding(3).Text("Total Expenses").Bold();
            table.Cell().BorderTop(1).Padding(3).AlignRight().Text($"{report.TotalExpenses:C}").Bold().FontColor(Colors.Red.Darken2);
        });

        // Net Income
        var netColor = report.NetIncome >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
        col.Item().PaddingTop(15).Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
        {
            row.RelativeItem().Text("NET INCOME").FontSize(14).Bold();
            row.ConstantItem(150).AlignRight().Text($"{report.NetIncome:C}").FontSize(16).Bold().FontColor(netColor);
        });
    }

    private static void RenderBalanceSheet(ColumnDescriptor col, BalanceSheetDto report)
    {
        col.Item().Text($"Balance Sheet - As of {report.AsOfDate:MMMM dd, yyyy}").Bold().FontSize(14);

        // Assets
        col.Item().PaddingTop(15).Text("ASSETS").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
        RenderAccountSection(col, "Current Assets", report.CurrentAssets, report.TotalCurrentAssets);
        RenderAccountSection(col, "Non-Current Assets", report.NonCurrentAssets, report.TotalNonCurrentAssets);
        col.Item().Background(Colors.Blue.Lighten4).Padding(5).Row(row =>
        {
            row.RelativeItem().Text("TOTAL ASSETS").Bold();
            row.ConstantItem(150).AlignRight().Text($"{report.TotalAssets:C}").Bold();
        });

        // Liabilities
        col.Item().PaddingTop(20).Text("LIABILITIES").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
        RenderAccountSection(col, "Current Liabilities", report.CurrentLiabilities, report.TotalCurrentLiabilities);
        RenderAccountSection(col, "Non-Current Liabilities", report.NonCurrentLiabilities, report.TotalNonCurrentLiabilities);
        col.Item().Background(Colors.Red.Lighten4).Padding(5).Row(row =>
        {
            row.RelativeItem().Text("TOTAL LIABILITIES").Bold();
            row.ConstantItem(150).AlignRight().Text($"{report.TotalLiabilities:C}").Bold();
        });

        // Equity
        col.Item().PaddingTop(20).Text("EQUITY").Bold().FontSize(12).FontColor(Colors.Green.Darken2);
        col.Item().Background(Colors.Green.Lighten4).Padding(5).Row(row =>
        {
            row.RelativeItem().Text("TOTAL EQUITY").Bold();
            row.ConstantItem(150).AlignRight().Text($"{report.TotalEquity:C}").Bold();
        });
    }

    private static void RenderAccountSection(ColumnDescriptor col, string title, IEnumerable<BalanceSheetLineDto>? items, decimal total)
    {
        col.Item().PaddingTop(10).Text(title).Bold().FontSize(11);
        if (items != null && items.Any())
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });
                foreach (var item in items)
                {
                    table.Cell().Padding(2).PaddingLeft(15).Text(item.AccountName);
                    table.Cell().Padding(2).AlignRight().Text($"{item.Balance:C}");
                }
                table.Cell().BorderTop(1).Padding(2).Text($"Total {title}").Italic();
                table.Cell().BorderTop(1).Padding(2).AlignRight().Text($"{total:C}").Italic();
            });
        }
    }

    private static void RenderCashFlowStatement(ColumnDescriptor col, CashFlowStatementDto report)
    {
        col.Item().Text($"Cash Flow Statement - {report.PeriodStart:MMM dd, yyyy} to {report.PeriodEnd:MMM dd, yyyy}").Bold().FontSize(14);

        RenderCashFlowSection(col, "Operating Activities", report.OperatingActivities, report.NetCashFromOperating, Colors.Blue.Darken2);
        RenderCashFlowSection(col, "Investing Activities", report.InvestingActivities, report.NetCashFromInvesting, Colors.Orange.Darken2);
        RenderCashFlowSection(col, "Financing Activities", report.FinancingActivities, report.NetCashFromFinancing, Colors.Purple.Darken2);

        col.Item().PaddingTop(15).Background(Colors.Grey.Lighten3).Padding(10).Column(summary =>
        {
            summary.Item().Row(r =>
            {
                r.RelativeItem().Text("Net Change in Cash");
                r.ConstantItem(150).AlignRight().Text($"{report.NetChangeInCash:C}").FontColor(report.NetChangeInCash >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
            });
            summary.Item().Row(r =>
            {
                r.RelativeItem().Text("Beginning Cash Balance");
                r.ConstantItem(150).AlignRight().Text($"{report.BeginningCashBalance:C}");
            });
            summary.Item().Row(r =>
            {
                r.RelativeItem().Text("Ending Cash Balance").Bold();
                r.ConstantItem(150).AlignRight().Text($"{report.EndingCashBalance:C}").Bold();
            });
        });
    }

    private static void RenderCashFlowSection(ColumnDescriptor col, string title, CashFlowSectionDto? section, decimal netAmount, string color)
    {
        col.Item().PaddingTop(15).Text(title).Bold().FontSize(12).FontColor(color);
        if (section?.Items != null && section.Items.Any())
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });
                foreach (var item in section.Items)
                {
                    table.Cell().Padding(2).PaddingLeft(15).Text(item.Description);
                    table.Cell().Padding(2).AlignRight().Text($"{item.Amount:C}");
                }
                table.Cell().BorderTop(1).Padding(2).Text($"Net from {title}").Bold();
                table.Cell().BorderTop(1).Padding(2).AlignRight().Text($"{netAmount:C}").Bold();
            });
        }
    }

    private static void RenderRevenueReport(ColumnDescriptor col, RevenueReportDto report)
    {
        col.Item().Text($"Revenue Report - {report.PeriodStart:MMM dd, yyyy} to {report.PeriodEnd:MMM dd, yyyy}").Bold().FontSize(14);

        col.Item().PaddingTop(15).Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
        {
            row.RelativeItem().Text("TOTAL REVENUE").FontSize(14).Bold();
            row.ConstantItem(150).AlignRight().Text($"{report.TotalRevenue:C}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
        });

        if (report.RevenueByCategory != null && report.RevenueByCategory.Any())
        {
            col.Item().PaddingTop(15).Text("Revenue by Category").Bold().FontSize(12);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Category").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Amount").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("%").FontColor(Colors.White).Bold();
                });
                foreach (var cat in report.RevenueByCategory)
                {
                    var pct = report.TotalRevenue > 0 ? (cat.Amount / report.TotalRevenue) * 100 : 0;
                    table.Cell().Padding(5).Text(cat.Category);
                    table.Cell().Padding(5).AlignRight().Text($"{cat.Amount:C}");
                    table.Cell().Padding(5).AlignRight().Text($"{pct:F1}%");
                }
            });
        }
    }

    public async Task<byte[]> ExportToExcelAsync<T>(T report, string reportType) where T : class
    {
        await Task.CompletedTask;
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportType.Length > 31 ? reportType[..31] : reportType);

        // Title
        worksheet.Cell(1, 1).Value = $"Financial Report: {reportType}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(2, 1).Value = $"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}";

        var row = 4;
        if (report is IncomeStatementDto income)
        {
            row = WriteIncomeStatementToExcel(worksheet, income, row);
        }
        else if (report is BalanceSheetDto balance)
        {
            row = WriteBalanceSheetToExcel(worksheet, balance, row);
        }
        else if (report is RevenueReportDto revenue)
        {
            row = WriteRevenueReportToExcel(worksheet, revenue, row);
        }
        else
        {
            // Generic JSON export
            worksheet.Cell(row, 1).Value = "Report Data (JSON)";
            worksheet.Cell(row + 1, 1).Value = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static int WriteIncomeStatementToExcel(IXLWorksheet worksheet, IncomeStatementDto report, int startRow)
    {
        var row = startRow;
        worksheet.Cell(row, 1).Value = $"Income Statement: {report.PeriodStart:MMM dd, yyyy} - {report.PeriodEnd:MMM dd, yyyy}";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "REVENUE";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var item in report.RevenueItems ?? Enumerable.Empty<IncomeStatementLineDto>())
        {
            worksheet.Cell(row, 1).Value = item.Description;
            worksheet.Cell(row, 2).Value = item.Amount;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }
        worksheet.Cell(row, 1).Value = "Total Revenue";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.TotalRevenue;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "EXPENSES";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var item in report.ExpenseItems ?? Enumerable.Empty<IncomeStatementLineDto>())
        {
            worksheet.Cell(row, 1).Value = item.Description;
            worksheet.Cell(row, 2).Value = item.Amount;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }
        worksheet.Cell(row, 1).Value = "Total Expenses";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.TotalExpenses;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "NET INCOME";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.NetIncome;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Style.Font.FontColor = report.NetIncome >= 0 ? XLColor.Green : XLColor.Red;

        return row + 2;
    }

    private static int WriteBalanceSheetToExcel(IXLWorksheet worksheet, BalanceSheetDto report, int startRow)
    {
        var row = startRow;
        worksheet.Cell(row, 1).Value = $"Balance Sheet: As of {report.AsOfDate:MMMM dd, yyyy}";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "ASSETS";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var item in report.CurrentAssets?.Concat(report.NonCurrentAssets ?? Enumerable.Empty<BalanceSheetLineDto>()) ?? Enumerable.Empty<BalanceSheetLineDto>())
        {
            worksheet.Cell(row, 1).Value = item.AccountName;
            worksheet.Cell(row, 2).Value = item.Balance;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }
        worksheet.Cell(row, 1).Value = "Total Assets";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.TotalAssets;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "LIABILITIES";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        foreach (var item in report.CurrentLiabilities?.Concat(report.NonCurrentLiabilities ?? Enumerable.Empty<BalanceSheetLineDto>()) ?? Enumerable.Empty<BalanceSheetLineDto>())
        {
            worksheet.Cell(row, 1).Value = item.AccountName;
            worksheet.Cell(row, 2).Value = item.Balance;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
            row++;
        }
        worksheet.Cell(row, 1).Value = "Total Liabilities";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.TotalLiabilities;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "Total Equity";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.TotalEquity;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;

        return row + 2;
    }

    private static int WriteRevenueReportToExcel(IXLWorksheet worksheet, RevenueReportDto report, int startRow)
    {
        var row = startRow;
        worksheet.Cell(row, 1).Value = $"Revenue Report: {report.PeriodStart:MMM dd, yyyy} - {report.PeriodEnd:MMM dd, yyyy}";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        worksheet.Cell(row, 1).Value = "Total Revenue";
        worksheet.Cell(row, 2).Value = report.TotalRevenue;
        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        row += 2;

        if (report.RevenueByCategory != null)
        {
            worksheet.Cell(row, 1).Value = "Category";
            worksheet.Cell(row, 2).Value = "Amount";
            worksheet.Cell(row, 3).Value = "Percentage";
            worksheet.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;

            foreach (var cat in report.RevenueByCategory)
            {
                var pct = report.TotalRevenue > 0 ? (cat.Amount / report.TotalRevenue) * 100 : 0;
                worksheet.Cell(row, 1).Value = cat.Category;
                worksheet.Cell(row, 2).Value = cat.Amount;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                worksheet.Cell(row, 3).Value = pct / 100;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "0.0%";
                row++;
            }
        }

        return row + 2;
    }

    public async Task<byte[]> ExportToCsvAsync<T>(T report, string reportType) where T : class
    {
        await Task.CompletedTask;
        var sb = new StringBuilder();
        sb.AppendLine($"Financial Report: {reportType}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        if (report is IncomeStatementDto income)
        {
            sb.AppendLine($"Income Statement,{income.PeriodStart:yyyy-MM-dd},{income.PeriodEnd:yyyy-MM-dd}");
            sb.AppendLine("Description,Amount");
            sb.AppendLine("REVENUE,");
            foreach (var item in income.RevenueItems ?? Enumerable.Empty<IncomeStatementLineDto>())
            {
                sb.AppendLine($"{EscapeCsv(item.Description)},{item.Amount}");
            }
            sb.AppendLine($"Total Revenue,{income.TotalRevenue}");
            sb.AppendLine("EXPENSES,");
            foreach (var item in income.ExpenseItems ?? Enumerable.Empty<IncomeStatementLineDto>())
            {
                sb.AppendLine($"{EscapeCsv(item.Description)},{item.Amount}");
            }
            sb.AppendLine($"Total Expenses,{income.TotalExpenses}");
            sb.AppendLine($"NET INCOME,{income.NetIncome}");
        }
        else if (report is BalanceSheetDto balance)
        {
            sb.AppendLine($"Balance Sheet,{balance.AsOfDate:yyyy-MM-dd}");
            sb.AppendLine("Account,Balance");
            sb.AppendLine("ASSETS,");
            foreach (var item in balance.CurrentAssets?.Concat(balance.NonCurrentAssets ?? Enumerable.Empty<BalanceSheetLineDto>()) ?? Enumerable.Empty<BalanceSheetLineDto>())
            {
                sb.AppendLine($"{EscapeCsv(item.AccountName)},{item.Balance}");
            }
            sb.AppendLine($"Total Assets,{balance.TotalAssets}");
            sb.AppendLine("LIABILITIES,");
            foreach (var item in balance.CurrentLiabilities?.Concat(balance.NonCurrentLiabilities ?? Enumerable.Empty<BalanceSheetLineDto>()) ?? Enumerable.Empty<BalanceSheetLineDto>())
            {
                sb.AppendLine($"{EscapeCsv(item.AccountName)},{item.Balance}");
            }
            sb.AppendLine($"Total Liabilities,{balance.TotalLiabilities}");
            sb.AppendLine($"Total Equity,{balance.TotalEquity}");
        }
        else if (report is RevenueReportDto revenue)
        {
            sb.AppendLine($"Revenue Report,{revenue.PeriodStart:yyyy-MM-dd},{revenue.PeriodEnd:yyyy-MM-dd}");
            sb.AppendLine($"Total Revenue,{revenue.TotalRevenue}");
            sb.AppendLine();
            sb.AppendLine("Category,Amount,Percentage");
            foreach (var cat in revenue.RevenueByCategory ?? Enumerable.Empty<RevenueByCategoryDto>())
            {
                var pct = revenue.TotalRevenue > 0 ? (cat.Amount / revenue.TotalRevenue) * 100 : 0;
                sb.AppendLine($"{EscapeCsv(cat.Category)},{cat.Amount},{pct:F1}%");
            }
        }
        else
        {
            sb.AppendLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    #endregion

    #region Private Helper Methods

    private static bool IsCurrentAsset(string accountCode)
    {
        // Current assets typically start with 1001-1099
        return accountCode.StartsWith("1001") || accountCode.StartsWith("1002") ||
               accountCode.StartsWith("1003") || accountCode.StartsWith("1004") ||
               accountCode.StartsWith("1005");
    }

    private static bool IsCurrentLiability(string accountCode)
    {
        // Current liabilities typically start with 2001-2099
        return accountCode.StartsWith("2001") || accountCode.StartsWith("2002") ||
               accountCode.StartsWith("2003") || accountCode.StartsWith("2004");
    }

    private static bool IsCostOfService(string accountCode)
    {
        // Cost of services typically start with 5XXX
        return accountCode.StartsWith("5");
    }

    private static void AddVarianceToSection(BalanceSheetSectionDto current, BalanceSheetSectionDto prior)
    {
        foreach (var item in current.Items)
        {
            var priorItem = prior.Items.FirstOrDefault(p => p.AccountCode == item.AccountCode);
            if (priorItem != null)
            {
                item.PriorPeriodBalance = priorItem.Balance;
                item.ChangeAmount = item.Balance - priorItem.Balance;
                item.ChangePercent = priorItem.Balance != 0 ? (item.ChangeAmount / priorItem.Balance) * 100 : null;
            }
        }
    }

    private static List<VarianceItemDto> CalculateVariances(EnhancedProfitLossReportDto current, EnhancedProfitLossReportDto prior)
    {
        var variances = new List<VarianceItemDto>
        {
            new()
            {
                ItemName = "Total Revenue",
                ActualAmount = current.TotalRevenue,
                ComparisonAmount = prior.TotalRevenue,
                VarianceAmount = current.TotalRevenue - prior.TotalRevenue,
                VariancePercent = prior.TotalRevenue != 0 ? ((current.TotalRevenue - prior.TotalRevenue) / prior.TotalRevenue) * 100 : 0,
                IsFavorable = current.TotalRevenue >= prior.TotalRevenue,
                VarianceType = "PriorPeriod"
            },
            new()
            {
                ItemName = "Gross Profit",
                ActualAmount = current.GrossProfit,
                ComparisonAmount = prior.GrossProfit,
                VarianceAmount = current.GrossProfit - prior.GrossProfit,
                VariancePercent = prior.GrossProfit != 0 ? ((current.GrossProfit - prior.GrossProfit) / prior.GrossProfit) * 100 : 0,
                IsFavorable = current.GrossProfit >= prior.GrossProfit,
                VarianceType = "PriorPeriod"
            },
            new()
            {
                ItemName = "Net Income",
                ActualAmount = current.NetIncome,
                ComparisonAmount = prior.NetIncome,
                VarianceAmount = current.NetIncome - prior.NetIncome,
                VariancePercent = prior.NetIncome != 0 ? ((current.NetIncome - prior.NetIncome) / prior.NetIncome) * 100 : 0,
                IsFavorable = current.NetIncome >= prior.NetIncome,
                VarianceType = "PriorPeriod"
            }
        };

        return variances;
    }

    private static string GetAgingBucket(int daysOutstanding)
    {
        return daysOutstanding switch
        {
            <= 0 => "Current",
            <= 30 => "1-30 Days",
            <= 60 => "31-60 Days",
            <= 90 => "61-90 Days",
            _ => "Over 90 Days"
        };
    }

    private async Task<decimal> GetCollectionsFromPatientsAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Where(p => p.BranchId == branchId && p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .SumAsync(p => p.Amount);
    }

    private async Task<decimal> GetPaymentsToSuppliersAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.SupplierPayments
            .Where(sp => sp.BranchId == branchId && sp.PaymentDate >= startDate && sp.PaymentDate <= endDate)
            .SumAsync(sp => sp.Amount);
    }

    private async Task<decimal> GetPaymentsToEmployeesAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Would need a payroll table - returning 0 for now
        await Task.CompletedTask;
        return 0;
    }

    private async Task<decimal> GetOtherOperatingExpensesAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Expenses
            .Where(e => e.BranchId == branchId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .SumAsync(e => e.Amount);
    }

    #endregion
}
