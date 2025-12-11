using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Payment Gateway Configuration DTOs

/// <summary>
/// DTO for payment gateway configuration
/// </summary>
public class PaymentGatewayConfigDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public PaymentGatewayProvider Provider { get; set; }
    public string ProviderName => Provider.ToString();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSandbox { get; set; }
    public string SupportedCurrencies { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public string SupportedMethods { get; set; } = string.Empty;
    public bool HasApiKey { get; set; }
    public bool HasWebhookSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating payment gateway configuration
/// </summary>
public class CreatePaymentGatewayConfigDto
{
    public PaymentGatewayProvider Provider { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsSandbox { get; set; } = true;
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public string? MerchantId { get; set; }
    public string? AccountId { get; set; }
    public string SupportedCurrencies { get; set; } = "AED,USD";
    public string DefaultCurrency { get; set; } = "AED";
    public string SupportedMethods { get; set; } = "card";
    public string? AdditionalConfig { get; set; }
}

/// <summary>
/// DTO for updating payment gateway configuration
/// </summary>
public class UpdatePaymentGatewayConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSandbox { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? WebhookSecret { get; set; }
    public string? MerchantId { get; set; }
    public string? AccountId { get; set; }
    public string SupportedCurrencies { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = string.Empty;
    public string SupportedMethods { get; set; } = string.Empty;
    public string? AdditionalConfig { get; set; }
}

#endregion

#region Payment Intent DTOs

/// <summary>
/// DTO for creating a payment intent
/// </summary>
public class CreatePaymentIntentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public int? InvoiceId { get; set; }
    public int? SaleId { get; set; }
    public int? PatientId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? Description { get; set; }
    public PaymentGatewayProvider? PreferredProvider { get; set; }
    public string? PaymentMethodType { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response from creating a payment intent
/// </summary>
public class PaymentIntentResponseDto
{
    public string TransactionReference { get; set; } = string.Empty;
    public string? GatewayPaymentIntentId { get; set; }
    public PaymentGatewayProvider Provider { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentUrl { get; set; }
    public PaymentGatewayStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// DTO for confirming a payment
/// </summary>
public class ConfirmPaymentDto
{
    public string TransactionReference { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public string? CardToken { get; set; }
    public string? ReturnUrl { get; set; }
}

#endregion

#region Payment Transaction DTOs

/// <summary>
/// DTO for payment gateway transaction
/// </summary>
public class PaymentGatewayTransactionDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public PaymentGatewayProvider Provider { get; set; }
    public string ProviderName => Provider.ToString();
    public string TransactionReference { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayPaymentIntentId { get; set; }
    public PaymentGatewayStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        PaymentGatewayStatus.Pending => "Pending",
        PaymentGatewayStatus.Processing => "Processing",
        PaymentGatewayStatus.Succeeded => "Succeeded",
        PaymentGatewayStatus.Failed => "Failed",
        PaymentGatewayStatus.Cancelled => "Cancelled",
        PaymentGatewayStatus.Refunded => "Refunded",
        PaymentGatewayStatus.PartiallyRefunded => "Partially Refunded",
        PaymentGatewayStatus.Disputed => "Disputed",
        PaymentGatewayStatus.RequiresAction => "Requires Action",
        PaymentGatewayStatus.RequiresCapture => "Requires Capture",
        PaymentGatewayStatus.Expired => "Expired",
        _ => "Unknown"
    };
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? GatewayFee { get; set; }
    public decimal? NetAmount { get; set; }
    public string PaymentMethodType { get; set; } = string.Empty;
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? SaleId { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Description { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal RefundedAmount { get; set; }
    public string? ThreeDSecureStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// DTO for listing payment transactions
/// </summary>
public class PaymentTransactionListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PaymentGatewayProvider? Provider { get; set; }
    public PaymentGatewayStatus? Status { get; set; }
    public int? PatientId { get; set; }
    public int? InvoiceId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Refund DTOs

/// <summary>
/// DTO for creating a refund
/// </summary>
public class CreateRefundDto
{
    public string TransactionReference { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response from creating a refund
/// </summary>
public class RefundResponseDto
{
    public string RefundId { get; set; } = string.Empty;
    public string OriginalTransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Webhook DTOs

/// <summary>
/// DTO for webhook event from payment gateway
/// </summary>
public class PaymentWebhookEventDto
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public PaymentGatewayProvider Provider { get; set; }
    public string? TransactionReference { get; set; }
    public string? GatewayTransactionId { get; set; }
    public PaymentGatewayStatus? NewStatus { get; set; }
    public string? RawPayload { get; set; }
    public DateTime ReceivedAt { get; set; }
}

#endregion

#region Payment Statistics DTOs

/// <summary>
/// Payment gateway statistics
/// </summary>
public class PaymentGatewayStatisticsDto
{
    public int TotalTransactions { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalSuccessfulAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal TotalFees { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public Dictionary<string, int> TransactionsByProvider { get; set; } = new();
    public Dictionary<string, decimal> AmountByProvider { get; set; } = new();
    public Dictionary<string, int> TransactionsByStatus { get; set; } = new();
    public Dictionary<string, int> TransactionsByPaymentMethod { get; set; } = new();
}

#endregion
