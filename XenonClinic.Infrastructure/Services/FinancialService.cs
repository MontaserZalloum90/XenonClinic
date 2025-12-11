using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Financial management
/// </summary>
public class FinancialService : IFinancialService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public FinancialService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
    }

    #region Account Management

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Branch)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Account>> GetAccountsByBranchIdAsync(int branchId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId)
            .OrderBy(a => a.AccountName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAccountsByTypeAsync(int branchId, AccountType accountType)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && a.AccountType == accountType)
            .OrderBy(a => a.AccountName)
            .ToListAsync();
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == account.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {account.BranchId} not found");
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        // Validate account exists
        var existingAccount = await _context.Accounts.FindAsync(account.Id);
        if (existingAccount == null)
        {
            throw new KeyNotFoundException($"Account with ID {account.Id} not found");
        }

        _context.Entry(existingAccount).CurrentValues.SetValues(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            // Check for linked transactions before deleting
            var hasTransactions = await _context.FinancialTransactions
                .AnyAsync(t => t.AccountId == id);

            if (hasTransactions)
            {
                throw new InvalidOperationException(
                    "Cannot delete account with existing transactions. Archive the account instead.");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Transaction Management

    public async Task<FinancialTransaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.FinancialTransactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByBranchIdAsync(int branchId)
    {
        return await _context.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.Account!.BranchId == branchId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountIdAsync(int accountId)
    {
        return await _context.FinancialTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Validate date range
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date", nameof(endDate));
        }

        return await _context.FinancialTransactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.Account!.BranchId == branchId &&
                   t.TransactionDate >= startDate &&
                   t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<FinancialTransaction> CreateTransactionAsync(FinancialTransaction transaction)
    {
        // Validate amount
        if (transaction.Amount <= 0)
        {
            throw new InvalidOperationException("Transaction amount must be greater than zero");
        }

        // Use database transaction to prevent race conditions
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _context.Accounts.FindAsync(transaction.AccountId);
            if (account == null)
            {
                throw new KeyNotFoundException($"Account with ID {transaction.AccountId} not found");
            }

            _context.FinancialTransactions.Add(transaction);

            // Update account balance atomically based on account type
            // For Assets/Expenses: Debits increase balance, Credits decrease balance
            // For Liabilities/Revenue/Equity: Credits increase balance, Debits decrease balance
            var balanceChange = CalculateBalanceChange(account.AccountType, transaction.TransactionType, transaction.Amount);
            account.Balance += balanceChange;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateTransactionAsync(FinancialTransaction transaction)
    {
        // Use database transaction for atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingTransaction = await _context.FinancialTransactions.FindAsync(transaction.Id);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found");
            }

            // Store original account ID for balance recalculation
            var originalAccountId = existingTransaction.AccountId;

            _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recalculate account balances (both original and new if changed)
            await UpdateAccountBalanceAsync(originalAccountId);
            if (transaction.AccountId != originalAccountId)
            {
                await UpdateAccountBalanceAsync(transaction.AccountId);
            }

            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteTransactionAsync(int id)
    {
        // Use database transaction for atomicity
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var transaction = await _context.FinancialTransactions.FindAsync(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found");
            }

            // Validate transaction can be deleted (not in a closed period)
            if (transaction.Status == VoucherStatus.Voided)
            {
                throw new InvalidOperationException("Cannot delete a voided transaction");
            }

            var accountId = transaction.AccountId;

            // Instead of hard delete, mark as voided for audit trail
            transaction.Status = VoucherStatus.Voided;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recalculate account balance
            await UpdateAccountBalanceAsync(accountId);

            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    private async Task UpdateAccountBalanceAsync(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            // Only include non-voided transactions in balance calculation
            var transactions = await _context.FinancialTransactions
                .Where(t => t.AccountId == accountId && t.Status != VoucherStatus.Voided)
                .ToListAsync();

            decimal balance = 0;
            foreach (var transaction in transactions)
            {
                balance += CalculateBalanceChange(account.AccountType, transaction.TransactionType, transaction.Amount);
            }

            account.Balance = Math.Round(balance, 2, MidpointRounding.AwayFromZero);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Calculates the balance change based on account type and transaction type.
    /// Follows double-entry bookkeeping rules:
    /// - Assets/Expenses: Debits increase, Credits decrease
    /// - Liabilities/Revenue/Equity: Credits increase, Debits decrease
    /// </summary>
    private static decimal CalculateBalanceChange(AccountType accountType, TransactionType transactionType, decimal amount)
    {
        var isDebitIncreaseAccount = accountType == AccountType.Asset || accountType == AccountType.Expense;

        return (isDebitIncreaseAccount, transactionType) switch
        {
            (true, TransactionType.Debit) => amount,    // Asset/Expense + Debit = increase
            (true, TransactionType.Credit) => -amount,  // Asset/Expense + Credit = decrease
            (false, TransactionType.Credit) => amount,  // Liability/Revenue/Equity + Credit = increase
            (false, TransactionType.Debit) => -amount,  // Liability/Revenue/Equity + Debit = decrease
            _ => 0
        };
    }

    #endregion

    #region Invoice Management

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Branch)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByBranchIdAsync(int branchId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.BranchId == branchId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(int branchId, InvoiceStatus status)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.BranchId == branchId && i.Status == status)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == invoice.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {invoice.BranchId} not found");
        }

        // Validate total amount
        if (invoice.TotalAmount <= 0)
        {
            throw new InvalidOperationException("Invoice total amount must be greater than zero");
        }

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task UpdateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoiceAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            // Prevent deleting paid invoices for audit compliance
            if (invoice.Status == InvoiceStatus.Paid)
            {
                throw new InvalidOperationException(
                    "Cannot delete a paid invoice. Paid invoices must be retained for audit purposes. " +
                    "Consider voiding the invoice instead.");
            }

            // Prevent deleting invoices with payments
            if (invoice.Status == InvoiceStatus.PartiallyPaid)
            {
                throw new InvalidOperationException(
                    "Cannot delete an invoice with partial payments. Void the invoice instead.");
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateInvoiceNumberAsync(int branchId)
    {
        return await _sequenceGenerator.GenerateInvoiceNumberAsync(branchId);
    }

    #endregion

    #region Expense Management

    public async Task<Expense?> GetExpenseByIdAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Branch)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Expense>> GetExpensesByBranchIdAsync(int branchId)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.BranchId == branchId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(int branchId, int categoryId)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Where(e => e.BranchId == branchId && e.CategoryId == categoryId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        // Validate amount
        if (expense.Amount <= 0)
        {
            throw new InvalidOperationException("Expense amount must be greater than zero");
        }

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == expense.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {expense.BranchId} not found");
        }

        // Validate category exists if specified
        if (expense.CategoryId.HasValue)
        {
            var categoryExists = await _context.ExpenseCategories.AnyAsync(c => c.Id == expense.CategoryId.Value);
            if (!categoryExists)
            {
                throw new KeyNotFoundException($"Expense category with ID {expense.CategoryId.Value} not found");
            }
        }

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task UpdateExpenseAsync(Expense expense)
    {
        _context.Expenses.Update(expense);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpenseAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense != null)
        {
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Statistics & Reporting

    public async Task<decimal> GetTotalRevenueAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Validate date range
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date", nameof(endDate));
        }

        return await _context.FinancialTransactions
            .Where(t => t.Account!.BranchId == branchId &&
                   t.TransactionType == TransactionType.Credit &&
                   t.TransactionDate >= startDate &&
                   t.TransactionDate <= endDate)
            .SumAsync(t => t.Amount);
    }

    public async Task<decimal> GetTotalExpensesAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Expenses
            .Where(e => e.BranchId == branchId &&
                   e.ExpenseDate >= startDate &&
                   e.ExpenseDate <= endDate)
            .SumAsync(e => e.Amount);
    }

    public async Task<decimal> GetNetProfitAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var revenue = await GetTotalRevenueAsync(branchId, startDate, endDate);
        var expenses = await GetTotalExpensesAsync(branchId, startDate, endDate);
        return revenue - expenses;
    }

    public async Task<decimal> GetAccountBalanceAsync(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        return account?.Balance ?? 0;
    }

    public async Task<int> GetUnpaidInvoicesCountAsync(int branchId)
    {
        return await _context.Invoices
            .CountAsync(i => i.BranchId == branchId && i.Status != InvoiceStatus.Paid);
    }

    public async Task<decimal> GetUnpaidInvoicesTotalAsync(int branchId)
    {
        return await _context.Invoices
            .Where(i => i.BranchId == branchId && i.Status != InvoiceStatus.Paid)
            .SumAsync(i => i.TotalAmount);
    }

    #endregion
}
