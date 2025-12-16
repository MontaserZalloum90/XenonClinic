using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Configuration for payment gateway integration per branch
/// </summary>
public class PaymentGatewayConfig : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Payment gateway provider
    /// </summary>
    public PaymentGatewayProvider Provider { get; set; }

    /// <summary>
    /// Display name for this configuration
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this gateway is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default gateway for the branch
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether to use sandbox/test mode
    /// </summary>
    public bool IsSandbox { get; set; } = true;

    /// <summary>
    /// Encrypted API key / Public key
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Encrypted API secret / Private key
    /// </summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// Encrypted secret key (alias for ApiSecret for compatibility)
    /// </summary>
    public string? EncryptedSecretKey { get => ApiSecret; set => ApiSecret = value; }

    /// <summary>
    /// Webhook secret for verifying callbacks
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Encrypted webhook secret (alias for WebhookSecret for compatibility)
    /// </summary>
    public string? EncryptedWebhookSecret { get => WebhookSecret; set => WebhookSecret = value; }

    /// <summary>
    /// Merchant ID (for providers that require it)
    /// </summary>
    public string? MerchantId { get; set; }

    /// <summary>
    /// Account ID (for PayPal, etc.)
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Supported currencies (comma-separated: USD,AED,EUR)
    /// </summary>
    public string SupportedCurrencies { get; set; } = "AED,USD";

    /// <summary>
    /// Default currency for transactions
    /// </summary>
    public string DefaultCurrency { get; set; } = "AED";

    /// <summary>
    /// Supported payment methods (comma-separated: card,bank,wallet)
    /// </summary>
    public string SupportedMethods { get; set; } = "card";

    /// <summary>
    /// Additional configuration as JSON
    /// </summary>
    public string? AdditionalConfig { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
