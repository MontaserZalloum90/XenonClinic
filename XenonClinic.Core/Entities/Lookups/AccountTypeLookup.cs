namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for account types (replaces AccountType enum).
/// Examples: Asset, Liability, Equity, Revenue, Expense
/// </summary>
public class AccountTypeLookup : SystemLookup
{
    /// <summary>
    /// Whether this account type has a normal debit balance.
    /// </summary>
    public bool NormalDebitBalance { get; set; } = true;

    /// <summary>
    /// Whether this account type appears in the balance sheet.
    /// </summary>
    public bool IsBalanceSheetAccount { get; set; } = true;

    // Navigation properties
    public ICollection<FinancialAccount> Accounts { get; set; } = new List<FinancialAccount>();
}
