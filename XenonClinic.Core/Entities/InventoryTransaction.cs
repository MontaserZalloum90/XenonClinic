using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; } // Positive for additions, negative for reductions
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // Reference Information
    public string? ReferenceNumber { get; set; } // Invoice number, order number, etc.
    public int? PatientId { get; set; } // If transaction is related to a patient
    public int? TransferToBranchId { get; set; } // If transaction is a transfer

    // Tracking
    public string? PerformedBy { get; set; } // User who performed the transaction
    public string? Notes { get; set; }
    public int QuantityAfterTransaction { get; set; } // Stock level after this transaction

    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;
    public Patient? Patient { get; set; }
    public Branch? TransferToBranch { get; set; }
}
