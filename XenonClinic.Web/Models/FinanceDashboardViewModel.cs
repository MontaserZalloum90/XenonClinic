namespace XenonClinic.Web.Models;

public class FinanceDashboardViewModel
{
    // Summary metrics
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }

    // Period metrics
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal PendingExpenses { get; set; }
    public int PendingExpenseCount { get; set; }

    // Recent transactions
    public List<FinancialTransactionDto> RecentTransactions { get; set; } = new();

    // Pending expenses
    public List<ExpenseDto> PendingExpensesList { get; set; } = new();

    // Top expense categories (this month)
    public Dictionary<string, decimal> TopExpenseCategories { get; set; } = new();
}
