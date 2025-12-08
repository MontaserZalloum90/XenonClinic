namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for inventory transaction types (replaces InventoryTransactionType enum).
/// Examples: Purchase, Sale, Adjustment, Transfer, Return
/// </summary>
public class InventoryTransactionTypeLookup : SystemLookup
{
    public bool IncreasesStock { get; set; } = true;
    public bool RequiresApproval { get; set; } = false;
    public bool RequiresReason { get; set; } = false;
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
