using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime? DueDate { get; set; }
    public SaleStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    // Customer
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    // Financials
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }

    // Additional Info
    public string BranchName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsFullyPaid { get; set; }
    public bool IsOverdue { get; set; }
}
