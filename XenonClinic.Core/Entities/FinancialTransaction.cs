using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class FinancialTransaction
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    public VoucherStatus Status { get; set; } = VoucherStatus.Posted;

    public string? Notes { get; set; }

    // Link to source documents
    public int? ExpenseId { get; set; }
    public Expense? Expense { get; set; }

    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}
