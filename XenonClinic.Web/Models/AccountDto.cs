using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class AccountDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string AccountTypeDisplay { get; set; } = string.Empty;
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? Description { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public int ChildAccountCount { get; set; }
    public int TransactionCount { get; set; }
}
