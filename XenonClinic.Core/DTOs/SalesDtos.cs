using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Sale DTOs

/// <summary>
/// DTO for displaying sale information.
/// </summary>
public class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime? DueDate { get; set; }
    public SaleStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => PaymentStatus.ToString();
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => Total - PaidAmount;
    public bool IsFullyPaid => PaidAmount >= Total && Total > 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsFullyPaid;
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int? QuotationId { get; set; }
    public string? QuotationNumber { get; set; }
    public int ItemCount { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for sale item information.
/// </summary>
public class SaleItemDto
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? ItemCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? SerialNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a new sale.
/// </summary>
public class CreateSaleDto
{
    public int PatientId { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int? QuotationId { get; set; }
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a sale item.
/// </summary>
public class CreateSaleItemDto
{
    public int? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? ItemCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? SerialNumber { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing sale.
/// </summary>
public class UpdateSaleDto
{
    public int Id { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
}

/// <summary>
/// DTO for sale list request with filtering and pagination.
/// </summary>
public class SaleListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public SaleStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? OverdueOnly { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Payment DTOs

/// <summary>
/// DTO for displaying payment information.
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();
    public int SaleId { get; set; }
    public string? SaleInvoiceNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public int? InstallmentNumber { get; set; }
    public int? TotalInstallments { get; set; }
    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for recording a payment.
/// </summary>
public class RecordSalePaymentDto
{
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsuranceClaimNumber { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public int? InstallmentNumber { get; set; }
    public int? TotalInstallments { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Quotation DTOs

/// <summary>
/// DTO for displaying quotation information.
/// </summary>
public class QuotationDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public QuotationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientMRN { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int ValidityDays { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow &&
                             (Status == QuotationStatus.Draft || Status == QuotationStatus.Sent);
    public bool CanConvertToSale => Status == QuotationStatus.Accepted;
    public DateTime? AcceptedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }
    public int ItemCount { get; set; }
    public List<QuotationItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for quotation item information.
/// </summary>
public class QuotationItemDto
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public int? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? ItemCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a new quotation.
/// </summary>
public class CreateQuotationDto
{
    public int PatientId { get; set; }
    public int ValidityDays { get; set; } = 30;
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public List<CreateQuotationItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a quotation item.
/// </summary>
public class CreateQuotationItemDto
{
    public int? InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public string? ItemCode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing quotation.
/// </summary>
public class UpdateQuotationDto
{
    public int Id { get; set; }
    public int ValidityDays { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
}

/// <summary>
/// DTO for sending a quotation.
/// </summary>
public class SendQuotationDto
{
    public int QuotationId { get; set; }
    public string? EmailAddress { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// DTO for accepting a quotation.
/// </summary>
public class AcceptQuotationDto
{
    public int QuotationId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for rejecting a quotation.
/// </summary>
public class RejectQuotationDto
{
    public int QuotationId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for quotation list request with filtering and pagination.
/// </summary>
public class QuotationListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? PatientId { get; set; }
    public QuotationStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? ExpiredOnly { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Statistics DTOs

/// <summary>
/// DTO for sales statistics.
/// </summary>
public class SalesStatisticsDto
{
    public int TotalSales { get; set; }
    public int DraftSales { get; set; }
    public int ConfirmedSales { get; set; }
    public int CompletedSales { get; set; }
    public int CancelledSales { get; set; }
    public int OverdueSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal AverageSaleValue { get; set; }
    public int TotalQuotations { get; set; }
    public int PendingQuotations { get; set; }
    public int AcceptedQuotations { get; set; }
    public int RejectedQuotations { get; set; }
    public decimal QuotationConversionRate { get; set; }
    public Dictionary<SaleStatus, int> SalesByStatus { get; set; } = new();
    public Dictionary<PaymentStatus, int> SalesByPaymentStatus { get; set; } = new();
    public Dictionary<QuotationStatus, int> QuotationsByStatus { get; set; } = new();
    public List<TopSellingItemDto> TopSellingItems { get; set; } = new();
}

/// <summary>
/// DTO for top selling item.
/// </summary>
public class TopSellingItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

#region Validation Messages

/// <summary>
/// Validation messages for sales operations.
/// </summary>
public static class SalesValidationMessages
{
    // Patient
    public const string PatientRequired = "Patient is required";
    public const string PatientInvalid = "Invalid patient ID";

    // Sale
    public const string SaleIdRequired = "Sale ID is required";
    public const string SaleStatusInvalid = "Invalid sale status";
    public const string SaleItemsRequired = "At least one item is required";

    // Items
    public const string ItemNameRequired = "Item name is required";
    public const string ItemNameTooLong = "Item name cannot exceed 200 characters";
    public const string QuantityInvalid = "Quantity must be greater than 0";
    public const string UnitPriceInvalid = "Unit price must be greater than 0";
    public const string InventoryItemInvalid = "Invalid inventory item ID";

    // Discount
    public const string DiscountPercentageInvalid = "Discount percentage must be between 0 and 100";

    // Tax
    public const string TaxPercentageInvalid = "Tax percentage must be between 0 and 100";

    // Payment
    public const string PaymentAmountInvalid = "Payment amount must be greater than 0";
    public const string PaymentMethodInvalid = "Invalid payment method";
    public const string PaymentExceedsBalance = "Payment amount cannot exceed remaining balance";

    // Quotation
    public const string QuotationIdRequired = "Quotation ID is required";
    public const string QuotationStatusInvalid = "Invalid quotation status";
    public const string ValidityDaysInvalid = "Validity days must be between 1 and 365";
    public const string RejectionReasonRequired = "Rejection reason is required";

    // Warranty
    public const string WarrantyMonthsInvalid = "Warranty months must be between 1 and 120";

    // Installment
    public const string InstallmentNumberInvalid = "Installment number must be greater than 0";
    public const string TotalInstallmentsInvalid = "Total installments must be greater than 0";
    public const string InstallmentNumberExceedsTotal = "Installment number cannot exceed total installments";

    // Pagination
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";

    // Date Range
    public const string DateRangeInvalid = "End date must be greater than or equal to start date";
}

#endregion
