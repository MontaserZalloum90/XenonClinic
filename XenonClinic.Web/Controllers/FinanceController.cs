using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

[Authorize]
public class FinanceController : Controller
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<FinanceController> _logger;

    public FinanceController(ClinicDbContext context, ILogger<FinanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Finance Dashboard
    public async Task<IActionResult> Index()
    {
        try
        {
            var branchId = GetUserBranchId();
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var viewModel = new FinanceDashboardViewModel
            {
                // Total balances by account type
                TotalAssets = await _context.Accounts
                    .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Asset && a.IsActive)
                    .SumAsync(a => a.Balance),

                TotalLiabilities = await _context.Accounts
                    .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Liability && a.IsActive)
                    .SumAsync(a => a.Balance),

                TotalEquity = await _context.Accounts
                    .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Equity && a.IsActive)
                    .SumAsync(a => a.Balance),

                TotalRevenue = await _context.Accounts
                    .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Revenue && a.IsActive)
                    .SumAsync(a => a.Balance),

                TotalExpenses = await _context.Accounts
                    .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Expense && a.IsActive)
                    .SumAsync(a => a.Balance),

                // Monthly revenue/expenses
                MonthlyRevenue = await _context.FinancialTransactions
                    .Where(t => t.BranchId == branchId &&
                               t.TransactionDate >= startOfMonth &&
                               t.Account!.AccountType == AccountType.Revenue &&
                               t.Status == VoucherStatus.Posted)
                    .SumAsync(t => t.Amount),

                MonthlyExpenses = await _context.Expenses
                    .Where(e => e.BranchId == branchId &&
                               e.ExpenseDate >= startOfMonth &&
                               e.Status == ExpenseStatus.Paid)
                    .SumAsync(e => e.Amount),

                // Pending expenses
                PendingExpenses = await _context.Expenses
                    .Where(e => e.BranchId == branchId && e.Status == ExpenseStatus.Pending)
                    .SumAsync(e => e.Amount),

                PendingExpenseCount = await _context.Expenses
                    .CountAsync(e => e.BranchId == branchId && e.Status == ExpenseStatus.Pending),

                // Recent transactions
                RecentTransactions = await _context.FinancialTransactions
                    .Where(t => t.BranchId == branchId)
                    .Include(t => t.Account)
                    .Include(t => t.Expense)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(10)
                    .Select(t => new FinancialTransactionDto
                    {
                        Id = t.Id,
                        TransactionNumber = t.TransactionNumber,
                        TransactionDate = t.TransactionDate,
                        AccountId = t.AccountId,
                        AccountName = t.Account!.AccountName,
                        AccountCode = t.Account.AccountCode,
                        TransactionType = t.TransactionType,
                        Amount = t.Amount,
                        Status = t.Status,
                        Description = t.Description,
                        ExpenseId = t.ExpenseId,
                        ExpenseNumber = t.Expense != null ? t.Expense.ExpenseNumber : null
                    })
                    .ToListAsync(),

                // Pending expenses list
                PendingExpensesList = await _context.Expenses
                    .Where(e => e.BranchId == branchId && e.Status == ExpenseStatus.Pending)
                    .Include(e => e.ExpenseCategory)
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(10)
                    .Select(e => new ExpenseDto
                    {
                        Id = e.Id,
                        ExpenseNumber = e.ExpenseNumber,
                        ExpenseDate = e.ExpenseDate,
                        ExpenseCategoryId = e.ExpenseCategoryId,
                        ExpenseCategoryName = e.ExpenseCategory!.Name,
                        Description = e.Description,
                        Amount = e.Amount,
                        Status = e.Status,
                        Vendor = e.Vendor
                    })
                    .ToListAsync(),

                // Top expense categories this month
                TopExpenseCategories = await _context.Expenses
                    .Where(e => e.BranchId == branchId &&
                               e.ExpenseDate >= startOfMonth &&
                               e.Status != ExpenseStatus.Rejected)
                    .Include(e => e.ExpenseCategory)
                    .GroupBy(e => e.ExpenseCategory!.Name)
                    .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                    .OrderByDescending(x => x.Total)
                    .Take(5)
                    .ToDictionaryAsync(x => x.Category, x => x.Total)
            };

            // Calculate net profit
            viewModel.NetProfit = viewModel.TotalRevenue - viewModel.TotalExpenses;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading finance dashboard");
            return View(new FinanceDashboardViewModel());
        }
    }

    // GET: Finance/Accounts
    public async Task<IActionResult> Accounts()
    {
        try
        {
            var branchId = GetUserBranchId();

            var accounts = await _context.Accounts
                .Where(a => a.BranchId == branchId)
                .Include(a => a.ParentAccount)
                .OrderBy(a => a.AccountCode)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType,
                    ParentAccountId = a.ParentAccountId,
                    ParentAccountName = a.ParentAccount != null ? a.ParentAccount.AccountName : null,
                    Description = a.Description,
                    Balance = a.Balance,
                    IsActive = a.IsActive,
                    ChildAccountCount = a.ChildAccounts.Count,
                    TransactionCount = a.Transactions.Count
                })
                .ToListAsync();

            return View(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts");
            return View(new List<AccountDto>());
        }
    }

    // GET: Finance/CreateAccount
    public async Task<IActionResult> CreateAccount()
    {
        await PopulateDropdowns();
        return View(new CreateAccountViewModel());
    }

    // POST: Finance/CreateAccount
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            // Check if account code already exists
            if (await _context.Accounts.AnyAsync(a => a.BranchId == branchId && a.AccountCode == model.AccountCode))
            {
                ModelState.AddModelError("AccountCode", "Account code already exists");
                await PopulateDropdowns();
                return View(model);
            }

            var account = new Account
            {
                AccountCode = model.AccountCode,
                AccountName = model.AccountName,
                AccountType = model.AccountType,
                ParentAccountId = model.ParentAccountId,
                Description = model.Description,
                Balance = model.Balance,
                IsActive = model.IsActive,
                BranchId = branchId,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account created successfully";
            return RedirectToAction(nameof(Accounts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError("", "Error creating account");
            await PopulateDropdowns();
            return View(model);
        }
    }

    // GET: Finance/ExpenseCategories
    public async Task<IActionResult> ExpenseCategories()
    {
        try
        {
            var branchId = GetUserBranchId();

            var categories = await _context.ExpenseCategories
                .Where(ec => ec.BranchId == branchId)
                .Include(ec => ec.Account)
                .OrderBy(ec => ec.Name)
                .Select(ec => new ExpenseCategoryDto
                {
                    Id = ec.Id,
                    Name = ec.Name,
                    Description = ec.Description,
                    AccountId = ec.AccountId,
                    AccountName = ec.Account != null ? ec.Account.AccountName : null,
                    IsActive = ec.IsActive,
                    ExpenseCount = ec.Expenses.Count
                })
                .ToListAsync();

            return View(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expense categories");
            return View(new List<ExpenseCategoryDto>());
        }
    }

    // GET: Finance/CreateExpenseCategory
    public async Task<IActionResult> CreateExpenseCategory()
    {
        await PopulateDropdowns();
        return View(new CreateExpenseCategoryViewModel());
    }

    // POST: Finance/CreateExpenseCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpenseCategory(CreateExpenseCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            var category = new ExpenseCategory
            {
                Name = model.Name,
                Description = model.Description,
                AccountId = model.AccountId,
                IsActive = model.IsActive,
                BranchId = branchId,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };

            _context.ExpenseCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense category created successfully";
            return RedirectToAction(nameof(ExpenseCategories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense category");
            ModelState.AddModelError("", "Error creating expense category");
            await PopulateDropdowns();
            return View(model);
        }
    }

    // GET: Finance/Expenses
    public async Task<IActionResult> Expenses()
    {
        try
        {
            var branchId = GetUserBranchId();

            var expenses = await _context.Expenses
                .Where(e => e.BranchId == branchId)
                .Include(e => e.ExpenseCategory)
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    ExpenseNumber = e.ExpenseNumber,
                    ExpenseDate = e.ExpenseDate,
                    ExpenseCategoryId = e.ExpenseCategoryId,
                    ExpenseCategoryName = e.ExpenseCategory!.Name,
                    Description = e.Description,
                    Amount = e.Amount,
                    Status = e.Status,
                    Vendor = e.Vendor,
                    InvoiceNumber = e.InvoiceNumber,
                    InvoiceDate = e.InvoiceDate,
                    PaymentMethod = e.PaymentMethod,
                    ReferenceNumber = e.ReferenceNumber,
                    PaymentDate = e.PaymentDate,
                    ApprovedBy = e.ApprovedBy,
                    ApprovedDate = e.ApprovedDate
                })
                .ToListAsync();

            return View(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
            return View(new List<ExpenseDto>());
        }
    }

    // GET: Finance/CreateExpense
    public async Task<IActionResult> CreateExpense()
    {
        await PopulateDropdowns();
        return View(new CreateExpenseViewModel());
    }

    // POST: Finance/CreateExpense
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpense(CreateExpenseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            var expense = new Expense
            {
                ExpenseNumber = await GenerateExpenseNumber(branchId),
                ExpenseDate = model.ExpenseDate,
                ExpenseCategoryId = model.ExpenseCategoryId,
                Description = model.Description,
                Amount = model.Amount,
                Status = ExpenseStatus.Pending,
                Vendor = model.Vendor,
                InvoiceNumber = model.InvoiceNumber,
                InvoiceDate = model.InvoiceDate,
                PaymentMethod = model.PaymentMethod,
                ReferenceNumber = model.ReferenceNumber,
                PaymentDate = model.PaymentDate,
                Notes = model.Notes,
                BranchId = branchId,
                CreatedBy = userName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense created successfully";
            return RedirectToAction(nameof(Expenses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            ModelState.AddModelError("", "Error creating expense");
            await PopulateDropdowns();
            return View(model);
        }
    }

    // POST: Finance/ApproveExpense
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveExpense(int id)
    {
        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .ThenInclude(ec => ec!.Account)
                .FirstOrDefaultAsync(e => e.Id == id && e.BranchId == branchId);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending expenses can be approved";
                return RedirectToAction(nameof(Expenses));
            }

            expense.Status = ExpenseStatus.Approved;
            expense.ApprovedBy = userName;
            expense.ApprovedDate = DateTime.UtcNow;
            expense.LastModifiedBy = userName;
            expense.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense approved successfully";
            return RedirectToAction(nameof(Expenses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving expense");
            TempData["ErrorMessage"] = "Error approving expense";
            return RedirectToAction(nameof(Expenses));
        }
    }

    // POST: Finance/PayExpense
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayExpense(int id)
    {
        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            var expense = await _context.Expenses
                .Include(e => e.ExpenseCategory)
                .ThenInclude(ec => ec!.Account)
                .FirstOrDefaultAsync(e => e.Id == id && e.BranchId == branchId);

            if (expense == null)
            {
                return NotFound();
            }

            if (expense.Status != ExpenseStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only approved expenses can be paid";
                return RedirectToAction(nameof(Expenses));
            }

            // Mark expense as paid
            expense.Status = ExpenseStatus.Paid;
            expense.PaymentDate = DateTime.UtcNow;
            expense.LastModifiedBy = userName;
            expense.LastModifiedAt = DateTime.UtcNow;

            // Create financial transaction if expense category has account
            if (expense.ExpenseCategory?.AccountId != null)
            {
                var transaction = new FinancialTransaction
                {
                    TransactionNumber = await GenerateTransactionNumber(branchId),
                    TransactionDate = DateTime.UtcNow,
                    AccountId = expense.ExpenseCategory.AccountId.Value,
                    TransactionType = TransactionType.Debit,
                    Amount = expense.Amount,
                    Status = VoucherStatus.Posted,
                    Description = $"Expense payment: {expense.Description}",
                    ReferenceNumber = expense.ExpenseNumber,
                    ExpenseId = expense.Id,
                    BranchId = branchId,
                    CreatedBy = userName,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FinancialTransactions.Add(transaction);

                // Update account balance
                var account = await _context.Accounts.FindAsync(expense.ExpenseCategory.AccountId.Value);
                if (account != null)
                {
                    account.Balance += expense.Amount; // Debit increases expense account
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense paid successfully";
            return RedirectToAction(nameof(Expenses));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error paying expense");
            TempData["ErrorMessage"] = "Error paying expense";
            return RedirectToAction(nameof(Expenses));
        }
    }

    // GET: Finance/Transactions
    public async Task<IActionResult> Transactions()
    {
        try
        {
            var branchId = GetUserBranchId();

            var transactions = await _context.FinancialTransactions
                .Where(t => t.BranchId == branchId)
                .Include(t => t.Account)
                .Include(t => t.Expense)
                .Include(t => t.Sale)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new FinancialTransactionDto
                {
                    Id = t.Id,
                    TransactionNumber = t.TransactionNumber,
                    TransactionDate = t.TransactionDate,
                    AccountId = t.AccountId,
                    AccountName = t.Account!.AccountName,
                    AccountCode = t.Account.AccountCode,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    Status = t.Status,
                    Description = t.Description,
                    ReferenceNumber = t.ReferenceNumber,
                    ExpenseId = t.ExpenseId,
                    ExpenseNumber = t.Expense != null ? t.Expense.ExpenseNumber : null,
                    SaleId = t.SaleId,
                    SaleNumber = t.Sale != null ? t.Sale.InvoiceNumber : null
                })
                .ToListAsync();

            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading transactions");
            return View(new List<FinancialTransactionDto>());
        }
    }

    // GET: Finance/CreateTransaction
    public async Task<IActionResult> CreateTransaction()
    {
        await PopulateDropdowns();
        return View(new CreateFinancialTransactionViewModel());
    }

    // POST: Finance/CreateTransaction
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(CreateFinancialTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        try
        {
            var branchId = GetUserBranchId();
            var userName = User.Identity?.Name ?? "System";

            var transaction = new FinancialTransaction
            {
                TransactionNumber = await GenerateTransactionNumber(branchId),
                TransactionDate = model.TransactionDate,
                AccountId = model.AccountId,
                TransactionType = model.TransactionType,
                Amount = model.Amount,
                Status = VoucherStatus.Posted,
                Description = model.Description,
                ReferenceNumber = model.ReferenceNumber,
                BranchId = branchId,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow
            };

            _context.FinancialTransactions.Add(transaction);

            // Update account balance
            var account = await _context.Accounts.FindAsync(model.AccountId);
            if (account != null)
            {
                if (model.TransactionType == TransactionType.Debit)
                {
                    // Debit increases: Assets, Expenses
                    // Debit decreases: Liabilities, Equity, Revenue
                    if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
                        account.Balance += model.Amount;
                    else
                        account.Balance -= model.Amount;
                }
                else // Credit
                {
                    // Credit increases: Liabilities, Equity, Revenue
                    // Credit decreases: Assets, Expenses
                    if (account.AccountType == AccountType.Liability ||
                        account.AccountType == AccountType.Equity ||
                        account.AccountType == AccountType.Revenue)
                        account.Balance += model.Amount;
                    else
                        account.Balance -= model.Amount;
                }

                account.LastModifiedBy = userName;
                account.LastModifiedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Transaction created successfully";
            return RedirectToAction(nameof(Transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            ModelState.AddModelError("", "Error creating transaction");
            await PopulateDropdowns();
            return View(model);
        }
    }

    // Helper Methods

    private async Task<string> GenerateExpenseNumber(int branchId)
    {
        var yearMonth = DateTime.Now.ToString("yyyyMM");
        var prefix = $"EXP-{yearMonth}-";

        var lastExpense = await _context.Expenses
            .Where(e => e.BranchId == branchId && e.ExpenseNumber.StartsWith(prefix))
            .OrderByDescending(e => e.ExpenseNumber)
            .FirstOrDefaultAsync();

        if (lastExpense == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastExpense.ExpenseNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }

    private async Task<string> GenerateTransactionNumber(int branchId)
    {
        var yearMonth = DateTime.Now.ToString("yyyyMM");
        var prefix = $"TRX-{yearMonth}-";

        var lastTransaction = await _context.FinancialTransactions
            .Where(t => t.BranchId == branchId && t.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync();

        if (lastTransaction == null)
        {
            return $"{prefix}0001";
        }

        var lastNumber = int.Parse(lastTransaction.TransactionNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }

    private async Task PopulateDropdowns()
    {
        var branchId = GetUserBranchId();

        // Parent accounts (for creating sub-accounts)
        ViewBag.ParentAccounts = await _context.Accounts
            .Where(a => a.BranchId == branchId && a.IsActive)
            .OrderBy(a => a.AccountName)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            })
            .ToListAsync();

        // Expense accounts for expense categories
        ViewBag.ExpenseAccounts = await _context.Accounts
            .Where(a => a.BranchId == branchId && a.AccountType == AccountType.Expense && a.IsActive)
            .OrderBy(a => a.AccountName)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            })
            .ToListAsync();

        // Expense categories
        ViewBag.ExpenseCategories = await _context.ExpenseCategories
            .Where(ec => ec.BranchId == branchId && ec.IsActive)
            .OrderBy(ec => ec.Name)
            .Select(ec => new SelectListItem
            {
                Value = ec.Id.ToString(),
                Text = ec.Name
            })
            .ToListAsync();

        // All active accounts
        ViewBag.Accounts = await _context.Accounts
            .Where(a => a.BranchId == branchId && a.IsActive)
            .OrderBy(a => a.AccountCode)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            })
            .ToListAsync();
    }

    private int GetUserBranchId()
    {
        // TODO: Implement proper branch resolution from user claims
        return 1;
    }
}
