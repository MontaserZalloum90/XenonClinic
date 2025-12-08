using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public string InventoryItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public int? TransferToBranchId { get; set; }
    public string? TransferToBranchName { get; set; }
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }
    public int QuantityAfterTransaction { get; set; }
}
