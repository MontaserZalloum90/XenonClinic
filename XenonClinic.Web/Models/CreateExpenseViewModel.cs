using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateExpenseViewModel
{
    [Required]
    [Display(Name = "Expense_Date")]
    public DateTime ExpenseDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Expense_Category")]
    public int ExpenseCategoryId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [StringLength(100)]
    [Display(Name = "Vendor")]
    public string? Vendor { get; set; }

    [StringLength(50)]
    [Display(Name = "Invoice_Number")]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Invoice_Date")]
    public DateTime? InvoiceDate { get; set; }

    [Display(Name = "Payment_Method")]
    public PaymentMethod? PaymentMethod { get; set; }

    [StringLength(100)]
    [Display(Name = "Reference_Number")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "Payment_Date")]
    public DateTime? PaymentDate { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
