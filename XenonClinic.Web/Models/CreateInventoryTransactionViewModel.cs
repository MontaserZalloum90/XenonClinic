using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateInventoryTransactionViewModel
{
    [Required(ErrorMessage = "Inventory item is required")]
    public int InventoryItemId { get; set; }

    [Required(ErrorMessage = "Transaction type is required")]
    public InventoryTransactionType TransactionType { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Unit price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than zero")]
    public decimal UnitPrice { get; set; }

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }

    public int? PatientId { get; set; }

    public int? TransferToBranchId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;
}
