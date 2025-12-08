using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateFinancialTransactionViewModel
{
    [Required]
    [Display(Name = "Transaction_Date")]
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    [Required]
    [Display(Name = "Transaction_Type")]
    public TransactionType TransactionType { get; set; }

    [Required]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Reference_Number")]
    public string? ReferenceNumber { get; set; }
}
