using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public GoodsReceiptStatus Status { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? PurchaseOrderNumber { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? SupplierInvoiceNumber { get; set; }
    public string? ReceivedBy { get; set; }
    public int ItemCount { get; set; }
}
