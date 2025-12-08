using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class ExpenseDto
{
    public int Id { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodDisplay { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
}
