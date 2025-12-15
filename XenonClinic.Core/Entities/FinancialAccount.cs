using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Chart of accounts entry
/// </summary>
public class FinancialAccount : IBranchEntity
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? AccountNameAr { get; set; }

    public int AccountTypeId { get; set; }
    public Lookups.AccountTypeLookup? AccountType { get; set; }

    public int? ParentAccountId { get; set; }
    public FinancialAccount? ParentAccount { get; set; }

    public int Level { get; set; }
    public bool IsHeader { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowTransactions { get; set; } = true;

    public string? Description { get; set; }
    public string? CurrencyCode { get; set; } = "SAR";

    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<FinancialAccount>? ChildAccounts { get; set; }
    public ICollection<VoucherLine>? VoucherLines { get; set; }
}
