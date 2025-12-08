using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreatePaymentViewModel
{
    [Required]
    public int SaleId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? BankName { get; set; }

    public string? CardLastFourDigits { get; set; }

    // Insurance fields
    public string? InsuranceCompany { get; set; }

    public string? InsuranceClaimNumber { get; set; }

    public string? InsurancePolicyNumber { get; set; }

    // Installment fields
    public int? InstallmentNumber { get; set; }

    public int? TotalInstallments { get; set; }

    public string? Notes { get; set; }
}
