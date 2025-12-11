using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Payment Gateway operations
/// </summary>
public interface IPaymentGatewayService
{
    #region Gateway Configuration

    /// <summary>
    /// Get all gateway configurations for a branch
    /// </summary>
    Task<IEnumerable<PaymentGatewayConfigDto>> GetConfigurationsAsync(int branchId);

    /// <summary>
    /// Get a specific gateway configuration
    /// </summary>
    Task<PaymentGatewayConfigDto?> GetConfigurationByIdAsync(int id);

    /// <summary>
    /// Get the default gateway configuration for a branch
    /// </summary>
    Task<PaymentGatewayConfigDto?> GetDefaultConfigurationAsync(int branchId);

    /// <summary>
    /// Create a new gateway configuration
    /// </summary>
    Task<PaymentGatewayConfigDto> CreateConfigurationAsync(int branchId, CreatePaymentGatewayConfigDto dto);

    /// <summary>
    /// Update a gateway configuration
    /// </summary>
    Task<PaymentGatewayConfigDto> UpdateConfigurationAsync(UpdatePaymentGatewayConfigDto dto);

    /// <summary>
    /// Delete a gateway configuration
    /// </summary>
    Task DeleteConfigurationAsync(int id);

    /// <summary>
    /// Test gateway connection
    /// </summary>
    Task<bool> TestConnectionAsync(int configId);

    #endregion

    #region Payment Operations

    /// <summary>
    /// Create a payment intent for a transaction
    /// </summary>
    Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int branchId, CreatePaymentIntentDto dto);

    /// <summary>
    /// Confirm a payment
    /// </summary>
    Task<PaymentGatewayTransactionDto> ConfirmPaymentAsync(ConfirmPaymentDto dto);

    /// <summary>
    /// Cancel a payment intent
    /// </summary>
    Task<PaymentGatewayTransactionDto> CancelPaymentAsync(string transactionReference);

    /// <summary>
    /// Capture an authorized payment
    /// </summary>
    Task<PaymentGatewayTransactionDto> CapturePaymentAsync(string transactionReference, decimal? amount = null);

    /// <summary>
    /// Process a refund
    /// </summary>
    Task<RefundResponseDto> RefundPaymentAsync(CreateRefundDto dto);

    /// <summary>
    /// Get payment status
    /// </summary>
    Task<PaymentGatewayTransactionDto> GetPaymentStatusAsync(string transactionReference);

    #endregion

    #region Transaction Queries

    /// <summary>
    /// Get a transaction by ID
    /// </summary>
    Task<PaymentGatewayTransactionDto?> GetTransactionByIdAsync(int id);

    /// <summary>
    /// Get a transaction by reference
    /// </summary>
    Task<PaymentGatewayTransactionDto?> GetTransactionByReferenceAsync(string transactionReference);

    /// <summary>
    /// Get transactions for a branch
    /// </summary>
    Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsAsync(int branchId, PaymentTransactionListRequestDto request);

    /// <summary>
    /// Get transactions for an invoice
    /// </summary>
    Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsByInvoiceAsync(int invoiceId);

    /// <summary>
    /// Get transactions for a patient
    /// </summary>
    Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsByPatientAsync(int patientId);

    #endregion

    #region Webhooks

    /// <summary>
    /// Process incoming webhook from payment gateway
    /// </summary>
    Task<bool> ProcessWebhookAsync(PaymentGatewayProvider provider, string payload, string signature);

    #endregion

    #region Statistics

    /// <summary>
    /// Get payment statistics for a branch
    /// </summary>
    Task<PaymentGatewayStatisticsDto> GetStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    #endregion

    #region Utility

    /// <summary>
    /// Generate a unique transaction reference
    /// </summary>
    Task<string> GenerateTransactionReferenceAsync(int branchId);

    /// <summary>
    /// Validate card number using Luhn algorithm
    /// </summary>
    bool ValidateCardNumber(string cardNumber);

    #endregion
}
