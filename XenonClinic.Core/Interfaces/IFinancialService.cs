using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Financial management
/// </summary>
public interface IFinancialService
{
    // Account Management
    Task<Account?> GetAccountByIdAsync(int id);
    Task<IEnumerable<Account>> GetAccountsByBranchIdAsync(int branchId);
    Task<IEnumerable<Account>> GetAccountsByTypeAsync(int branchId, AccountType accountType);
    Task<Account> CreateAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeleteAccountAsync(int id);

    // Transaction Management
    Task<FinancialTransaction?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByBranchIdAsync(int branchId);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountIdAsync(int accountId);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<FinancialTransaction> CreateTransactionAsync(FinancialTransaction transaction);
    Task UpdateTransactionAsync(FinancialTransaction transaction);
    Task DeleteTransactionAsync(int id);

    // Invoice Management
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetInvoicesByBranchIdAsync(int branchId);
    Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(int branchId, InvoiceStatus status);
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task DeleteInvoiceAsync(int id);
    Task<string> GenerateInvoiceNumberAsync(int branchId);

    // Expense Management
    Task<Expense?> GetExpenseByIdAsync(int id);
    Task<IEnumerable<Expense>> GetExpensesByBranchIdAsync(int branchId);
    Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int branchId, int categoryId);
    Task<Expense> CreateExpenseAsync(Expense expense);
    Task UpdateExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(int id);

    // Statistics & Reporting
    Task<decimal> GetTotalRevenueAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalExpensesAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetNetProfitAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetAccountBalanceAsync(int accountId);
    Task<int> GetUnpaidInvoicesCountAsync(int branchId);
    Task<decimal> GetUnpaidInvoicesTotalAsync(int branchId);
}
