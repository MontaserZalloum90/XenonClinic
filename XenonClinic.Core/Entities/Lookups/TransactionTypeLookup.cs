namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for transaction types (replaces TransactionType enum).
/// Examples: Debit, Credit
/// </summary>
public class TransactionTypeLookup : SystemLookup
{
    public bool IsDebitTransaction { get; set; } = true;
    public int Multiplier { get; set; } = 1; // 1 for debit, -1 for credit
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}
