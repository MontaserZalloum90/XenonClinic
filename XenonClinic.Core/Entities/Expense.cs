using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class Expense
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ExpenseNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    [MaxLength(100)]
    public string? Vendor { get; set; }

    [MaxLength(50)]
    public string? InvoiceNumber { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }

    [MaxLength(450)]
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
