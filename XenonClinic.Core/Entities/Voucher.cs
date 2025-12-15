using System.ComponentModel.DataAnnotations.Schema;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Financial voucher for journal entries
/// </summary>
[Table("Vouchers")]
public class Voucher : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string VoucherNumber { get; set; } = string.Empty;
    public int VoucherStatusId { get; set; }
    public Lookups.VoucherStatusLookup? VoucherStatus { get; set; }

    public DateTime VoucherDate { get; set; }
    public DateTime? PostingDate { get; set; }

    public string? Reference { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<VoucherLine>? Lines { get; set; }
}

/// <summary>
/// Individual line in a voucher
/// </summary>
[Table("VoucherLines")]
public class VoucherLine
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    public int AccountId { get; set; }
    public FinancialAccount? Account { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }

    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }

    public int LineNumber { get; set; }
}
