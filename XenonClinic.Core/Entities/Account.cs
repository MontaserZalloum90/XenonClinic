using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

public class Account
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    public AccountType AccountType { get; set; }

    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal Balance { get; set; }

    public bool IsActive { get; set; } = true;

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}
