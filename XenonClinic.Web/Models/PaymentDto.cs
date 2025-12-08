using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class PaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? CardLastFourDigits { get; set; }

    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
