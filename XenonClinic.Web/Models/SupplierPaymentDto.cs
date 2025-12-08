using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class SupplierPaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SupplierPaymentStatus Status { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string PaidBy { get; set; } = string.Empty;
}
