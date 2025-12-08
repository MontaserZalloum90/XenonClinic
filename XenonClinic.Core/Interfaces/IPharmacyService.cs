using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Pharmacy/Sales management
/// </summary>
public interface IPharmacyService
{
    // Sale Management
    Task<Sale?> GetSaleByIdAsync(int id);
    Task<Sale?> GetSaleByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Sale>> GetSalesByBranchIdAsync(int branchId);
    Task<IEnumerable<Sale>> GetSalesByPatientIdAsync(int patientId);
    Task<IEnumerable<Sale>> GetSalesByStatusAsync(int branchId, SaleStatus status);
    Task<IEnumerable<Sale>> GetSalesByPaymentStatusAsync(int branchId, PaymentStatus paymentStatus);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetOverdueSalesAsync(int branchId);
    Task<Sale> CreateSaleAsync(Sale sale);
    Task UpdateSaleAsync(Sale sale);
    Task DeleteSaleAsync(int id);
    Task<string> GenerateInvoiceNumberAsync(int branchId);

    // Sale Status Management
    Task ConfirmSaleAsync(int saleId);
    Task CompleteSaleAsync(int saleId);
    Task CancelSaleAsync(int saleId);

    // Sale Item Management
    Task<SaleItem?> GetSaleItemByIdAsync(int id);
    Task<IEnumerable<SaleItem>> GetSaleItemsAsync(int saleId);
    Task<SaleItem> AddSaleItemAsync(SaleItem saleItem);
    Task UpdateSaleItemAsync(SaleItem saleItem);
    Task DeleteSaleItemAsync(int id);

    // Payment Management
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<IEnumerable<Payment>> GetPaymentsBySaleIdAsync(int saleId);
    Task<Payment> AddPaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);

    // Quotation Management
    Task<Quotation?> GetQuotationByIdAsync(int id);
    Task<IEnumerable<Quotation>> GetQuotationsByBranchIdAsync(int branchId);
    Task<IEnumerable<Quotation>> GetQuotationsByPatientIdAsync(int patientId);
    Task<Quotation> CreateQuotationAsync(Quotation quotation);
    Task UpdateQuotationAsync(Quotation quotation);
    Task DeleteQuotationAsync(int id);
    Task<Sale> ConvertQuotationToSaleAsync(int quotationId);

    // Statistics & Reporting
    Task<int> GetTotalSalesCountAsync(int branchId);
    Task<decimal> GetTotalSalesRevenueAsync(int branchId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalOutstandingBalanceAsync(int branchId);
    Task<int> GetOverdueSalesCountAsync(int branchId);
    Task<Dictionary<SaleStatus, int>> GetSalesStatusDistributionAsync(int branchId);
    Task<Dictionary<PaymentStatus, int>> GetSalesPaymentStatusDistributionAsync(int branchId);
    Task<IEnumerable<(string ItemName, int Quantity, decimal Revenue)>> GetTopSellingItemsAsync(int branchId, int topN = 10);
}
