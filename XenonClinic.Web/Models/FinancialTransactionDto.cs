using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class FinancialTransactionDto
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public VoucherStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public int? ExpenseId { get; set; }
    public string? ExpenseNumber { get; set; }
    public int? SaleId { get; set; }
    public string? SaleNumber { get; set; }
}
