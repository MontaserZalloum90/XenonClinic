using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Payment Gateway operations
/// </summary>
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly ClinicDbContext _context;
    private readonly ISecretEncryptionService _encryptionService;
    private readonly ISequenceGenerator _sequenceGenerator;
    private readonly HttpClient _httpClient;

    public PaymentGatewayService(
        ClinicDbContext context,
        ISecretEncryptionService encryptionService,
        ISequenceGenerator sequenceGenerator,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _encryptionService = encryptionService;
        _sequenceGenerator = sequenceGenerator;
        _httpClient = httpClientFactory.CreateClient("PaymentGateway");
    }

    #region Gateway Configuration

    public async Task<IEnumerable<PaymentGatewayConfigDto>> GetConfigurationsAsync(int branchId)
    {
        var configs = await _context.Set<PaymentGatewayConfig>()
            .AsNoTracking()
            .Include(c => c.Branch)
            .Where(c => c.BranchId == branchId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return configs.Select(MapToConfigDto);
    }

    public async Task<PaymentGatewayConfigDto?> GetConfigurationByIdAsync(int id)
    {
        var config = await _context.Set<PaymentGatewayConfig>()
            .AsNoTracking()
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);

        return config != null ? MapToConfigDto(config) : null;
    }

    public async Task<PaymentGatewayConfigDto?> GetDefaultConfigurationAsync(int branchId)
    {
        var config = await _context.Set<PaymentGatewayConfig>()
            .AsNoTracking()
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.IsDefault && c.IsActive);

        return config != null ? MapToConfigDto(config) : null;
    }

    public async Task<PaymentGatewayConfigDto> CreateConfigurationAsync(int branchId, CreatePaymentGatewayConfigDto dto)
    {
        // If this is set as default, unset other defaults
        if (dto.IsDefault)
        {
            await UnsetDefaultConfigurationsAsync(branchId);
        }

        var config = new PaymentGatewayConfig
        {
            BranchId = branchId,
            Provider = dto.Provider,
            Name = dto.Name,
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault,
            IsSandbox = dto.IsSandbox,
            ApiKey = !string.IsNullOrEmpty(dto.ApiKey) ? _encryptionService.Encrypt(dto.ApiKey) : null,
            ApiSecret = !string.IsNullOrEmpty(dto.ApiSecret) ? _encryptionService.Encrypt(dto.ApiSecret) : null,
            WebhookSecret = !string.IsNullOrEmpty(dto.WebhookSecret) ? _encryptionService.Encrypt(dto.WebhookSecret) : null,
            MerchantId = dto.MerchantId,
            AccountId = dto.AccountId,
            SupportedCurrencies = dto.SupportedCurrencies,
            DefaultCurrency = dto.DefaultCurrency,
            SupportedMethods = dto.SupportedMethods,
            AdditionalConfig = dto.AdditionalConfig,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<PaymentGatewayConfig>().Add(config);
        await _context.SaveChangesAsync();

        return MapToConfigDto(config);
    }

    public async Task<PaymentGatewayConfigDto> UpdateConfigurationAsync(UpdatePaymentGatewayConfigDto dto)
    {
        var config = await _context.Set<PaymentGatewayConfig>()
            .FirstOrDefaultAsync(c => c.Id == dto.Id);

        if (config == null)
        {
            throw new KeyNotFoundException($"Payment gateway configuration with ID {dto.Id} not found");
        }

        // If this is set as default, unset other defaults
        if (dto.IsDefault && !config.IsDefault)
        {
            await UnsetDefaultConfigurationsAsync(config.BranchId);
        }

        config.Name = dto.Name;
        config.IsActive = dto.IsActive;
        config.IsDefault = dto.IsDefault;
        config.IsSandbox = dto.IsSandbox;

        if (!string.IsNullOrEmpty(dto.ApiKey))
        {
            config.ApiKey = _encryptionService.Encrypt(dto.ApiKey);
        }

        if (!string.IsNullOrEmpty(dto.ApiSecret))
        {
            config.ApiSecret = _encryptionService.Encrypt(dto.ApiSecret);
        }

        if (!string.IsNullOrEmpty(dto.WebhookSecret))
        {
            config.WebhookSecret = _encryptionService.Encrypt(dto.WebhookSecret);
        }

        config.MerchantId = dto.MerchantId;
        config.AccountId = dto.AccountId;
        config.SupportedCurrencies = dto.SupportedCurrencies;
        config.DefaultCurrency = dto.DefaultCurrency;
        config.SupportedMethods = dto.SupportedMethods;
        config.AdditionalConfig = dto.AdditionalConfig;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToConfigDto(config);
    }

    public async Task DeleteConfigurationAsync(int id)
    {
        var config = await _context.Set<PaymentGatewayConfig>().FindAsync(id);
        if (config != null)
        {
            _context.Set<PaymentGatewayConfig>().Remove(config);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> TestConnectionAsync(int configId)
    {
        var config = await _context.Set<PaymentGatewayConfig>().FindAsync(configId);
        if (config == null)
        {
            return false;
        }

        try
        {
            // Test connection based on provider
            return config.Provider switch
            {
                PaymentGatewayProvider.Stripe => await TestStripeConnectionAsync(config),
                PaymentGatewayProvider.PayPal => await TestPayPalConnectionAsync(config),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Payment Operations

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int branchId, CreatePaymentIntentDto dto)
    {
        var config = dto.PreferredProvider.HasValue
            ? await _context.Set<PaymentGatewayConfig>()
                .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Provider == dto.PreferredProvider && c.IsActive)
            : await _context.Set<PaymentGatewayConfig>()
                .FirstOrDefaultAsync(c => c.BranchId == branchId && c.IsDefault && c.IsActive);

        if (config == null)
        {
            throw new InvalidOperationException("No active payment gateway configuration found");
        }

        var transactionRef = await GenerateTransactionReferenceAsync(branchId);
        var amountInSmallestUnit = ConvertToSmallestUnit(dto.Amount, dto.Currency);

        // Create transaction record
        var transaction = new PaymentGatewayTransaction
        {
            BranchId = branchId,
            GatewayConfigId = config.Id,
            Provider = config.Provider,
            TransactionReference = transactionRef,
            Status = PaymentGatewayStatus.Pending,
            AmountInSmallestUnit = amountInSmallestUnit,
            Amount = dto.Amount,
            Currency = dto.Currency,
            PaymentMethodType = dto.PaymentMethodType ?? "card",
            InvoiceId = dto.InvoiceId,
            SaleId = dto.SaleId,
            PatientId = dto.PatientId,
            CustomerEmail = dto.CustomerEmail,
            CustomerName = dto.CustomerName,
            Description = dto.Description,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null,
            CreatedAt = DateTime.UtcNow
        };

        // Create payment intent with provider
        var result = config.Provider switch
        {
            PaymentGatewayProvider.Stripe => await CreateStripePaymentIntentAsync(config, transaction, dto),
            PaymentGatewayProvider.PayPal => await CreatePayPalOrderAsync(config, transaction, dto),
            _ => throw new NotSupportedException($"Provider {config.Provider} is not supported")
        };

        _context.Set<PaymentGatewayTransaction>().Add(transaction);
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<PaymentGatewayTransactionDto> ConfirmPaymentAsync(ConfirmPaymentDto dto)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .Include(t => t.GatewayConfig)
            .FirstOrDefaultAsync(t => t.TransactionReference == dto.TransactionReference);

        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction {dto.TransactionReference} not found");
        }

        if (transaction.Status != PaymentGatewayStatus.Pending &&
            transaction.Status != PaymentGatewayStatus.RequiresAction)
        {
            throw new InvalidOperationException($"Transaction is not in a confirmable state: {transaction.Status}");
        }

        // Confirm with provider
        transaction = transaction.Provider switch
        {
            PaymentGatewayProvider.Stripe => await ConfirmStripePaymentAsync(transaction, dto),
            PaymentGatewayProvider.PayPal => await CapturePayPalOrderAsync(transaction),
            _ => throw new NotSupportedException($"Provider {transaction.Provider} is not supported")
        };

        await _context.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    public async Task<PaymentGatewayTransactionDto> CancelPaymentAsync(string transactionReference)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .Include(t => t.GatewayConfig)
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);

        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction {transactionReference} not found");
        }

        transaction.Status = PaymentGatewayStatus.Cancelled;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    public async Task<PaymentGatewayTransactionDto> CapturePaymentAsync(string transactionReference, decimal? amount = null)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .Include(t => t.GatewayConfig)
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);

        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction {transactionReference} not found");
        }

        if (transaction.Status != PaymentGatewayStatus.RequiresCapture)
        {
            throw new InvalidOperationException($"Transaction is not authorized for capture: {transaction.Status}");
        }

        // Capture with provider
        transaction.Status = PaymentGatewayStatus.Succeeded;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToTransactionDto(transaction);
    }

    public async Task<RefundResponseDto> RefundPaymentAsync(CreateRefundDto dto)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .Include(t => t.GatewayConfig)
            .FirstOrDefaultAsync(t => t.TransactionReference == dto.TransactionReference);

        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction {dto.TransactionReference} not found");
        }

        if (transaction.Status != PaymentGatewayStatus.Succeeded)
        {
            throw new InvalidOperationException("Can only refund successful transactions");
        }

        var refundAmount = dto.Amount ?? transaction.Amount;
        if (refundAmount > (transaction.Amount - transaction.RefundedAmount))
        {
            throw new InvalidOperationException("Refund amount exceeds available balance");
        }

        // Process refund with provider
        var refundId = Guid.NewGuid().ToString("N")[..12].ToUpper();

        transaction.RefundedAmount += refundAmount;
        transaction.Status = transaction.RefundedAmount >= transaction.Amount
            ? PaymentGatewayStatus.Refunded
            : PaymentGatewayStatus.PartiallyRefunded;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RefundResponseDto
        {
            RefundId = refundId,
            OriginalTransactionReference = transaction.TransactionReference,
            Amount = refundAmount,
            Currency = transaction.Currency,
            Status = "succeeded",
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<PaymentGatewayTransactionDto> GetPaymentStatusAsync(string transactionReference)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.GatewayConfig)
            .Include(t => t.Invoice)
            .Include(t => t.Patient)
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);

        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction {transactionReference} not found");
        }

        return MapToTransactionDto(transaction);
    }

    #endregion

    #region Transaction Queries

    public async Task<PaymentGatewayTransactionDto?> GetTransactionByIdAsync(int id)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.Invoice)
            .Include(t => t.Patient)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction != null ? MapToTransactionDto(transaction) : null;
    }

    public async Task<PaymentGatewayTransactionDto?> GetTransactionByReferenceAsync(string transactionReference)
    {
        var transaction = await _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.Invoice)
            .Include(t => t.Patient)
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);

        return transaction != null ? MapToTransactionDto(transaction) : null;
    }

    public async Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsAsync(int branchId, PaymentTransactionListRequestDto request)
    {
        var query = _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.Invoice)
            .Include(t => t.Patient)
            .Where(t => t.BranchId == branchId);

        if (request.Provider.HasValue)
        {
            query = query.Where(t => t.Provider == request.Provider.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.PatientId.HasValue)
        {
            query = query.Where(t => t.PatientId == request.PatientId.Value);
        }

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(t => t.InvoiceId == request.InvoiceId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.DateTo.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(t =>
                t.TransactionReference.Contains(request.SearchTerm) ||
                t.CustomerEmail!.Contains(request.SearchTerm) ||
                t.CustomerName!.Contains(request.SearchTerm));
        }

        query = request.SortDescending
            ? query.OrderByDescending(t => EF.Property<object>(t, request.SortBy))
            : query.OrderBy(t => EF.Property<object>(t, request.SortBy));

        var transactions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return transactions.Select(MapToTransactionDto);
    }

    public async Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsByInvoiceAsync(int invoiceId)
    {
        var transactions = await _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.Patient)
            .Where(t => t.InvoiceId == invoiceId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapToTransactionDto);
    }

    public async Task<IEnumerable<PaymentGatewayTransactionDto>> GetTransactionsByPatientAsync(int patientId)
    {
        var transactions = await _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Include(t => t.Invoice)
            .Where(t => t.PatientId == patientId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return transactions.Select(MapToTransactionDto);
    }

    #endregion

    #region Webhooks

    public async Task<bool> ProcessWebhookAsync(PaymentGatewayProvider provider, string payload, string signature)
    {
        try
        {
            // Find config for this provider to verify signature
            var config = await _context.Set<PaymentGatewayConfig>()
                .FirstOrDefaultAsync(c => c.Provider == provider && c.IsActive);

            if (config == null)
            {
                return false;
            }

            // Verify webhook signature based on provider
            var isValid = provider switch
            {
                PaymentGatewayProvider.Stripe => VerifyStripeWebhookSignature(payload, signature, config),
                PaymentGatewayProvider.PayPal => VerifyPayPalWebhookSignature(payload, signature, config),
                _ => false
            };

            if (!isValid)
            {
                return false;
            }

            // Process the webhook event
            var webhookEvent = JsonSerializer.Deserialize<JsonElement>(payload);
            var eventType = webhookEvent.GetProperty("type").GetString();

            // Update transaction based on event
            // This is a simplified implementation
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Statistics

    public async Task<PaymentGatewayStatisticsDto> GetStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<PaymentGatewayTransaction>()
            .AsNoTracking()
            .Where(t => t.BranchId == branchId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var transactions = await query.ToListAsync();

        var stats = new PaymentGatewayStatisticsDto
        {
            TotalTransactions = transactions.Count,
            SuccessfulTransactions = transactions.Count(t => t.Status == PaymentGatewayStatus.Succeeded),
            FailedTransactions = transactions.Count(t => t.Status == PaymentGatewayStatus.Failed),
            PendingTransactions = transactions.Count(t => t.Status == PaymentGatewayStatus.Pending),
            TotalAmount = transactions.Sum(t => t.Amount),
            TotalSuccessfulAmount = transactions.Where(t => t.Status == PaymentGatewayStatus.Succeeded).Sum(t => t.Amount),
            TotalRefundedAmount = transactions.Sum(t => t.RefundedAmount),
            TotalFees = transactions.Where(t => t.GatewayFee.HasValue).Sum(t => t.GatewayFee!.Value),
            TransactionsByProvider = transactions.GroupBy(t => t.Provider.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            AmountByProvider = transactions.GroupBy(t => t.Provider.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
            TransactionsByStatus = transactions.GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TransactionsByPaymentMethod = transactions.GroupBy(t => t.PaymentMethodType)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        stats.SuccessRate = stats.TotalTransactions > 0
            ? (decimal)stats.SuccessfulTransactions / stats.TotalTransactions * 100
            : 0;

        stats.AverageTransactionAmount = stats.SuccessfulTransactions > 0
            ? stats.TotalSuccessfulAmount / stats.SuccessfulTransactions
            : 0;

        return stats;
    }

    #endregion

    #region Utility

    public async Task<string> GenerateTransactionReferenceAsync(int branchId)
    {
        var prefix = "TXN";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"{prefix}-{branchId}-{timestamp}-{random}";
    }

    public bool ValidateCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        // Remove spaces and dashes
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (!cardNumber.All(char.IsDigit) || cardNumber.Length < 13 || cardNumber.Length > 19)
        {
            return false;
        }

        // Luhn algorithm
        int sum = 0;
        bool alternate = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = int.Parse(cardNumber[i].ToString());

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    #endregion

    #region Private Methods

    private async Task UnsetDefaultConfigurationsAsync(int branchId)
    {
        var configs = await _context.Set<PaymentGatewayConfig>()
            .Where(c => c.BranchId == branchId && c.IsDefault)
            .ToListAsync();

        foreach (var config in configs)
        {
            config.IsDefault = false;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private static long ConvertToSmallestUnit(decimal amount, string currency)
    {
        // Most currencies use 2 decimal places, some use 0 (JPY) or 3 (KWD)
        var multiplier = currency.ToUpper() switch
        {
            "JPY" or "KRW" => 1,
            "KWD" or "BHD" or "OMR" => 1000,
            _ => 100
        };

        return (long)(amount * multiplier);
    }

    private static PaymentGatewayConfigDto MapToConfigDto(PaymentGatewayConfig config)
    {
        return new PaymentGatewayConfigDto
        {
            Id = config.Id,
            BranchId = config.BranchId,
            BranchName = config.Branch?.Name,
            Provider = config.Provider,
            Name = config.Name,
            IsActive = config.IsActive,
            IsDefault = config.IsDefault,
            IsSandbox = config.IsSandbox,
            SupportedCurrencies = config.SupportedCurrencies,
            DefaultCurrency = config.DefaultCurrency,
            SupportedMethods = config.SupportedMethods,
            HasApiKey = !string.IsNullOrEmpty(config.ApiKey),
            HasWebhookSecret = !string.IsNullOrEmpty(config.WebhookSecret),
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }

    private static PaymentGatewayTransactionDto MapToTransactionDto(PaymentGatewayTransaction transaction)
    {
        return new PaymentGatewayTransactionDto
        {
            Id = transaction.Id,
            BranchId = transaction.BranchId,
            Provider = transaction.Provider,
            TransactionReference = transaction.TransactionReference,
            GatewayTransactionId = transaction.GatewayTransactionId,
            GatewayPaymentIntentId = transaction.GatewayPaymentIntentId,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            GatewayFee = transaction.GatewayFee,
            NetAmount = transaction.NetAmount,
            PaymentMethodType = transaction.PaymentMethodType,
            CardBrand = transaction.CardBrand,
            CardLast4 = transaction.CardLast4,
            InvoiceId = transaction.InvoiceId,
            InvoiceNumber = transaction.Invoice?.InvoiceNumber,
            SaleId = transaction.SaleId,
            PatientId = transaction.PatientId,
            PatientName = transaction.Patient != null ? $"{transaction.Patient.FirstName} {transaction.Patient.LastName}" : null,
            CustomerEmail = transaction.CustomerEmail,
            Description = transaction.Description,
            ErrorCode = transaction.ErrorCode,
            ErrorMessage = transaction.ErrorMessage,
            RefundedAmount = transaction.RefundedAmount,
            ThreeDSecureStatus = transaction.ThreeDSecureStatus,
            CreatedAt = transaction.CreatedAt,
            ProcessedAt = transaction.ProcessedAt,
            CompletedAt = transaction.CompletedAt
        };
    }

    // Provider-specific implementations (simplified)
    private async Task<bool> TestStripeConnectionAsync(PaymentGatewayConfig config)
    {
        // In production, this would make an API call to Stripe
        await Task.Delay(100);
        return !string.IsNullOrEmpty(config.ApiKey);
    }

    private async Task<bool> TestPayPalConnectionAsync(PaymentGatewayConfig config)
    {
        // In production, this would make an API call to PayPal
        await Task.Delay(100);
        return !string.IsNullOrEmpty(config.ApiKey) && !string.IsNullOrEmpty(config.ApiSecret);
    }

    private async Task<PaymentIntentResponseDto> CreateStripePaymentIntentAsync(
        PaymentGatewayConfig config,
        PaymentGatewayTransaction transaction,
        CreatePaymentIntentDto dto)
    {
        // In production, this would create a real Stripe PaymentIntent
        await Task.Delay(100);

        var clientSecret = $"pi_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}";
        transaction.GatewayPaymentIntentId = $"pi_{Guid.NewGuid():N}";

        return new PaymentIntentResponseDto
        {
            TransactionReference = transaction.TransactionReference,
            GatewayPaymentIntentId = transaction.GatewayPaymentIntentId,
            Provider = PaymentGatewayProvider.Stripe,
            ClientSecret = clientSecret,
            Status = PaymentGatewayStatus.Pending,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    private async Task<PaymentIntentResponseDto> CreatePayPalOrderAsync(
        PaymentGatewayConfig config,
        PaymentGatewayTransaction transaction,
        CreatePaymentIntentDto dto)
    {
        // In production, this would create a real PayPal Order
        await Task.Delay(100);

        var orderId = Guid.NewGuid().ToString("N")[..17].ToUpper();
        transaction.GatewayPaymentIntentId = orderId;

        var baseUrl = config.IsSandbox
            ? "https://www.sandbox.paypal.com"
            : "https://www.paypal.com";

        return new PaymentIntentResponseDto
        {
            TransactionReference = transaction.TransactionReference,
            GatewayPaymentIntentId = orderId,
            Provider = PaymentGatewayProvider.PayPal,
            PaymentUrl = $"{baseUrl}/checkoutnow?token={orderId}",
            Status = PaymentGatewayStatus.Pending,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            ExpiresAt = DateTime.UtcNow.AddHours(3)
        };
    }

    private async Task<PaymentGatewayTransaction> ConfirmStripePaymentAsync(
        PaymentGatewayTransaction transaction,
        ConfirmPaymentDto dto)
    {
        // In production, this would confirm the Stripe PaymentIntent
        await Task.Delay(100);

        transaction.Status = PaymentGatewayStatus.Succeeded;
        transaction.GatewayTransactionId = $"ch_{Guid.NewGuid():N}";
        transaction.ProcessedAt = DateTime.UtcNow;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    private async Task<PaymentGatewayTransaction> CapturePayPalOrderAsync(PaymentGatewayTransaction transaction)
    {
        // In production, this would capture the PayPal Order
        await Task.Delay(100);

        transaction.Status = PaymentGatewayStatus.Succeeded;
        transaction.GatewayTransactionId = $"PAY-{Guid.NewGuid():N}"[..24].ToUpper();
        transaction.ProcessedAt = DateTime.UtcNow;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    private bool VerifyStripeWebhookSignature(string payload, string signature, PaymentGatewayConfig config)
    {
        // In production, this would verify the Stripe webhook signature
        return !string.IsNullOrEmpty(config.WebhookSecret);
    }

    private bool VerifyPayPalWebhookSignature(string payload, string signature, PaymentGatewayConfig config)
    {
        // In production, this would verify the PayPal webhook signature
        return !string.IsNullOrEmpty(config.WebhookSecret);
    }

    #endregion
}
