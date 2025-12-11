using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Account DTOs

/// <summary>
/// DTO for account data transfer.
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string AccountTypeDisplay => AccountType switch
    {
        AccountType.Asset => "Asset",
        AccountType.Liability => "Liability",
        AccountType.Equity => "Equity",
        AccountType.Revenue => "Revenue",
        AccountType.Expense => "Expense",
        _ => "Unknown"
    };
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? Description { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating an account.
/// </summary>
public class CreateAccountDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public string? Description { get; set; }
    public decimal InitialBalance { get; set; }
}

/// <summary>
/// DTO for updating an account.
/// </summary>
public class UpdateAccountDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Invoice DTOs

/// <summary>
/// DTO for invoice data transfer.
/// </summary>
public class InvoiceDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    // SECURITY FIX: Removed PatientEmiratesId - PII should not be exposed in invoice responses
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        InvoiceStatus.Draft => "Draft",
        InvoiceStatus.Issued => "Issued",
        InvoiceStatus.PartiallyPaid => "Partially Paid",
        InvoiceStatus.Paid => "Paid",
        InvoiceStatus.Overdue => "Overdue",
        InvoiceStatus.Cancelled => "Cancelled",
        InvoiceStatus.Refunded => "Refunded",
        _ => "Unknown"
    };
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodDisplay => PaymentMethod switch
    {
        Enums.PaymentMethod.Cash => "Cash",
        Enums.PaymentMethod.Card => "Card",
        Enums.PaymentMethod.BankTransfer => "Bank Transfer",
        Enums.PaymentMethod.Cheque => "Cheque",
        Enums.PaymentMethod.Insurance => "Insurance",
        _ => null
    };
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int? SaleId { get; set; }
    public bool IsFullyPaid => PaidAmount >= TotalAmount && TotalAmount > 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsFullyPaid;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating an invoice.
/// </summary>
public class CreateInvoiceDto
{
    public int PatientId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default 5%
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int? SaleId { get; set; }
}

/// <summary>
/// DTO for updating an invoice.
/// </summary>
public class UpdateInvoiceDto
{
    public int Id { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
}

/// <summary>
/// DTO for recording invoice payment.
/// </summary>
public class RecordPaymentDto
{
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for invoice list request with filters.
/// </summary>
public class InvoiceListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public InvoiceStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "InvoiceDate";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Expense DTOs

/// <summary>
/// DTO for expense data transfer.
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        ExpenseStatus.Pending => "Pending",
        ExpenseStatus.Approved => "Approved",
        ExpenseStatus.Paid => "Paid",
        ExpenseStatus.Rejected => "Rejected",
        _ => "Unknown"
    };
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating an expense.
/// </summary>
public class CreateExpenseDto
{
    public DateTime ExpenseDate { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// DTO for updating an expense.
/// </summary>
public class UpdateExpenseDto
{
    public int Id { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int ExpenseCategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// DTO for approving an expense.
/// </summary>
public class ApproveExpenseDto
{
    public int ExpenseId { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// DTO for rejecting an expense.
/// </summary>
public class RejectExpenseDto
{
    public int ExpenseId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for expense list request with filters.
/// </summary>
public class ExpenseListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CategoryId { get; set; }
    public ExpenseStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "ExpenseDate";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Transaction DTOs

/// <summary>
/// DTO for financial transaction data transfer.
/// </summary>
public class FinancialTransactionDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountCode { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay => TransactionType switch
    {
        TransactionType.Debit => "Debit",
        TransactionType.Credit => "Credit",
        _ => "Unknown"
    };
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public VoucherStatus Status { get; set; }
    public string? Notes { get; set; }
    public int? ExpenseId { get; set; }
    public int? SaleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for creating a financial transaction.
/// </summary>
public class CreateTransactionDto
{
    public DateTime TransactionDate { get; set; }
    public int AccountId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? ExpenseId { get; set; }
    public int? SaleId { get; set; }
}

/// <summary>
/// DTO for transaction list request with filters.
/// </summary>
public class TransactionListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? AccountId { get; set; }
    public TransactionType? TransactionType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "TransactionDate";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Financial Statistics DTOs

/// <summary>
/// DTO for financial dashboard statistics.
/// </summary>
public class FinancialStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public int UnpaidInvoicesCount { get; set; }
    public decimal UnpaidInvoicesTotal { get; set; }
    public int PendingExpensesCount { get; set; }
    public decimal PendingExpensesTotal { get; set; }
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    public Dictionary<string, decimal> AccountBalances { get; set; } = new();
}

/// <summary>
/// DTO for profit/loss report.
/// </summary>
public class ProfitLossReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public List<RevenueLineItemDto> RevenueItems { get; set; } = new();
    public List<ExpenseLineItemDto> ExpenseItems { get; set; } = new();
}

/// <summary>
/// DTO for revenue line item in reports.
/// </summary>
public class RevenueLineItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for expense line item in reports.
/// </summary>
public class ExpenseLineItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

#endregion

#region Validation Messages

/// <summary>
/// Standard validation error messages for financial operations.
/// </summary>
public static class FinancialValidationMessages
{
    // Account messages
    public const string AccountNotFound = "Account not found";
    public const string AccountCodeRequired = "Account code is required";
    public const string AccountCodeTooLong = "Account code cannot exceed 50 characters";
    public const string AccountCodeDuplicate = "An account with this code already exists";
    public const string AccountNameRequired = "Account name is required";
    public const string AccountNameTooLong = "Account name cannot exceed 200 characters";
    public const string AccountTypeRequired = "Account type is required";
    public const string AccountTypeInvalid = "Invalid account type";
    public const string ParentAccountInvalid = "Invalid parent account";
    public const string InitialBalanceInvalid = "Initial balance cannot be negative";

    // Invoice messages
    public const string InvoiceNotFound = "Invoice not found";
    public const string InvoiceNumberDuplicate = "An invoice with this number already exists";
    public const string PatientRequired = "Patient is required";
    public const string PatientNotFound = "Patient not found";
    public const string SubTotalRequired = "Subtotal is required";
    public const string SubTotalInvalid = "Subtotal must be greater than 0";
    public const string DiscountPercentageInvalid = "Discount percentage must be between 0 and 100";
    public const string TaxPercentageInvalid = "Tax percentage must be between 0 and 100";
    public const string DueDateInvalid = "Due date cannot be in the past";
    public const string InvoiceAlreadyPaid = "This invoice has already been paid";
    public const string PaymentExceedsRemaining = "Payment amount exceeds remaining balance";

    // Expense messages
    public const string ExpenseNotFound = "Expense not found";
    public const string ExpenseDateRequired = "Expense date is required";
    public const string ExpenseDateFuture = "Expense date cannot be in the future";
    public const string CategoryRequired = "Expense category is required";
    public const string CategoryNotFound = "Expense category not found";
    public const string DescriptionRequired = "Description is required";
    public const string DescriptionTooLong = "Description cannot exceed 200 characters";
    public const string AmountRequired = "Amount is required";
    public const string AmountInvalid = "Amount must be greater than 0";
    public const string ExpenseAlreadyApproved = "This expense has already been approved";
    public const string ExpenseAlreadyRejected = "This expense has already been rejected";
    public const string RejectionReasonRequired = "Rejection reason is required";

    // Transaction messages
    public const string TransactionNotFound = "Transaction not found";
    public const string TransactionDateRequired = "Transaction date is required";
    public const string AccountRequired = "Account is required";
    public const string TransactionTypeRequired = "Transaction type is required";
    public const string TransactionTypeInvalid = "Invalid transaction type";
    public const string TransactionDescriptionRequired = "Transaction description is required";

    // General messages
    public const string BranchAccessDenied = "You do not have access to this branch";
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";
    public const string DateRangeInvalid = "End date must be after start date";
}

#endregion
