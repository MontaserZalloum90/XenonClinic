using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Sales management including sales, quotations, and payments
/// </summary>
public interface ISalesService
{
    #region Sale Management

    Task<Sale?> GetSaleByIdAsync(int id);
    Task<Sale?> GetSaleByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Sale>> GetSalesByBranchIdAsync(int branchId);
    Task<IEnumerable<Sale>> GetSalesByPatientIdAsync(int patientId);
    Task<IEnumerable<Sale>> GetSalesByStatusAsync(int branchId, SaleStatus status);
    Task<IEnumerable<Sale>> GetSalesByPaymentStatusAsync(int branchId, PaymentStatus status);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetOverdueSalesAsync(int branchId);
    Task<Sale> CreateSaleAsync(Sale sale);
    Task UpdateSaleAsync(Sale sale);
    Task DeleteSaleAsync(int id);
    Task<Sale> ConfirmSaleAsync(int id);
    Task<Sale> CompleteSaleAsync(int id);
    Task<Sale> CancelSaleAsync(int id, string? cancellationReason = null);
    Task<string> GenerateSaleInvoiceNumberAsync(int branchId);

    #endregion

    #region Sale Items

    Task<SaleItem?> GetSaleItemByIdAsync(int id);
    Task<IEnumerable<SaleItem>> GetSaleItemsBySaleIdAsync(int saleId);
    Task<SaleItem> AddSaleItemAsync(SaleItem saleItem);
    Task UpdateSaleItemAsync(SaleItem saleItem);
    Task DeleteSaleItemAsync(int id);
    Task RecalculateSaleTotalsAsync(int saleId);

    #endregion

    #region Payment Management

    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<Payment?> GetPaymentByNumberAsync(string paymentNumber);
    Task<IEnumerable<Payment>> GetPaymentsBySaleIdAsync(int saleId);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<Payment> RecordPaymentAsync(Payment payment);
    Task<Payment> RecordPaymentAsync(int saleId, decimal amount, PaymentMethod method, string? referenceNumber = null);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
    Task RefundPaymentAsync(int paymentId, decimal refundAmount, string? reason = null);
    Task<string> GeneratePaymentNumberAsync(int branchId);

    #endregion

    #region Quotation Management

    Task<Quotation?> GetQuotationByIdAsync(int id);
    Task<Quotation?> GetQuotationByNumberAsync(string quotationNumber);
    Task<IEnumerable<Quotation>> GetQuotationsByBranchIdAsync(int branchId);
    Task<IEnumerable<Quotation>> GetQuotationsByPatientIdAsync(int patientId);
    Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(int branchId, QuotationStatus status);
    Task<IEnumerable<Quotation>> GetActiveQuotationsAsync(int branchId);
    Task<IEnumerable<Quotation>> GetExpiredQuotationsAsync(int branchId);
    Task<Quotation> CreateQuotationAsync(Quotation quotation);
    Task UpdateQuotationAsync(Quotation quotation);
    Task DeleteQuotationAsync(int id);
    Task<Quotation> SendQuotationAsync(int id);
    Task<Quotation> AcceptQuotationAsync(int id);
    Task<Quotation> RejectQuotationAsync(int id, string? reason = null);
    Task<Sale> ConvertQuotationToSaleAsync(int quotationId);
    Task<string> GenerateQuotationNumberAsync(int branchId);

    #endregion

    #region Quotation Items

    Task<QuotationItem?> GetQuotationItemByIdAsync(int id);
    Task<IEnumerable<QuotationItem>> GetQuotationItemsByQuotationIdAsync(int quotationId);
    Task<QuotationItem> AddQuotationItemAsync(QuotationItem quotationItem);
    Task UpdateQuotationItemAsync(QuotationItem quotationItem);
    Task DeleteQuotationItemAsync(int id);
    Task RecalculateQuotationTotalsAsync(int quotationId);

    #endregion

    #region Statistics & Reporting

    Task<decimal> GetTotalSalesAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalPaidAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalOutstandingAsync(int branchId);
    Task<int> GetOverdueSalesCountAsync(int branchId);
    Task<decimal> GetOverdueTotalAsync(int branchId);
    Task<int> GetPendingQuotationsCountAsync(int branchId);
    Task<decimal> GetQuotationConversionRateAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<SalesStatistics> GetSalesStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    #endregion
}

/// <summary>
/// Sales statistics data transfer object
/// </summary>
public class SalesStatistics
{
    public decimal TotalSales { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int SalesCount { get; set; }
    public int PendingSalesCount { get; set; }
    public int CompletedSalesCount { get; set; }
    public int OverdueSalesCount { get; set; }
    public decimal OverdueTotal { get; set; }
    public int PendingQuotationsCount { get; set; }
    public int AcceptedQuotationsCount { get; set; }
    public decimal QuotationConversionRate { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public Dictionary<PaymentMethod, decimal> PaymentMethodDistribution { get; set; } = new();
}
