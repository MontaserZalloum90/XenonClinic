using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public decimal Total { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal Balance { get; set; }
    public bool IsFullyReceived { get; set; }
    public string? SupplierInvoiceNumber { get; set; }
}
