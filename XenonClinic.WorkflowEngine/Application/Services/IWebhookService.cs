using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for managing webhooks - both inbound (receiving) and outbound (sending).
/// </summary>
public interface IWebhookService
{
    #region Webhook Registration

    /// <summary>
    /// Registers an outbound webhook subscription.
    /// </summary>
    Task<WebhookSubscription> RegisterSubscriptionAsync(RegisterWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing webhook subscription.
    /// </summary>
    Task<WebhookSubscription> UpdateSubscriptionAsync(string subscriptionId, UpdateWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook subscription.
    /// </summary>
    Task DeleteSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a webhook subscription by ID.
    /// </summary>
    Task<WebhookSubscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all webhook subscriptions, optionally filtered by tenant and/or event type.
    /// </summary>
    Task<IList<WebhookSubscription>> ListSubscriptionsAsync(string? tenantId = null, string? eventType = null, CancellationToken cancellationToken = default);

    #endregion

    #region Inbound Webhooks

    /// <summary>
    /// Registers an inbound webhook endpoint.
    /// </summary>
    Task<InboundWebhook> RegisterInboundWebhookAsync(RegisterInboundWebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an incoming webhook request.
    /// </summary>
    Task<WebhookProcessingResult> ProcessInboundWebhookAsync(string webhookId, InboundWebhookPayload payload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an inbound webhook by ID.
    /// </summary>
    Task<InboundWebhook?> GetInboundWebhookAsync(string webhookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates webhook signature.
    /// </summary>
    Task<bool> ValidateSignatureAsync(string webhookId, string signature, string payload, CancellationToken cancellationToken = default);

    #endregion

    #region Outbound Webhooks

    /// <summary>
    /// Sends a webhook notification for an event.
    /// </summary>
    Task NotifyWebhooksAsync(WorkflowEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed webhook delivery.
    /// </summary>
    Task<WebhookDeliveryResult> RetryDeliveryAsync(string deliveryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets delivery history for a subscription.
    /// </summary>
    Task<IList<WebhookDelivery>> GetDeliveryHistoryAsync(string subscriptionId, int limit = 50, CancellationToken cancellationToken = default);

    #endregion
}

#region Request/Response DTOs

public class RegisterWebhookRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public List<string> EventTypes { get; set; } = new(); // e.g., ["process.completed", "task.*"]
    public string? Secret { get; set; }
    public WebhookAuthConfig? Authentication { get; set; }
    public Dictionary<string, string> CustomHeaders { get; set; } = new();
    public bool Active { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}

public class UpdateWebhookRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? TargetUrl { get; set; }
    public List<string>? EventTypes { get; set; }
    public string? Secret { get; set; }
    public WebhookAuthConfig? Authentication { get; set; }
    public Dictionary<string, string>? CustomHeaders { get; set; }
    public bool? Active { get; set; }
}

public class RegisterInboundWebhookRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public string? SignatureHeader { get; set; } = "X-Webhook-Signature";
    public string? SignatureAlgorithm { get; set; } = "HMACSHA256";
    public WebhookAction Action { get; set; } = new();
    public Dictionary<string, string> PayloadMapping { get; set; } = new();
    public bool Active { get; set; } = true;
}

public class InboundWebhookPayload
{
    public string ContentType { get; set; } = "application/json";
    public string? RawBody { get; set; }
    public Dictionary<string, object>? JsonBody { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public string? Signature { get; set; }
    public string SourceIp { get; set; } = string.Empty;
}

#endregion

#region Entity DTOs

public class WebhookSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public List<string> EventTypes { get; set; } = new();
    public string? SecretHash { get; set; }
    public WebhookAuthConfig? Authentication { get; set; }
    public Dictionary<string, string> CustomHeaders { get; set; } = new();
    public bool Active { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastDeliveryAt { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
}

public class WebhookAuthConfig
{
    public string Type { get; set; } = "None"; // None, Basic, Bearer, ApiKey
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeyHeader { get; set; } = "X-Api-Key";
}

public class InboundWebhook
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty; // Generated endpoint URL
    public string? SecretHash { get; set; }
    public string? SignatureHeader { get; set; }
    public string? SignatureAlgorithm { get; set; }
    public WebhookAction Action { get; set; } = new();
    public Dictionary<string, string> PayloadMapping { get; set; } = new();
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ReceivedCount { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
}

public class WebhookAction
{
    public string Type { get; set; } = "StartProcess"; // StartProcess, SendMessage, SendSignal, TriggerEvent
    public string? ProcessDefinitionKey { get; set; }
    public string? MessageName { get; set; }
    public string? SignalName { get; set; }
    public string? ProcessInstanceId { get; set; }
    public Dictionary<string, string> VariableMapping { get; set; } = new();
}

public class WebhookDelivery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SubscriptionId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ResponseTime { get; set; }
}

public class WebhookDeliveryResult
{
    public string DeliveryId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public bool WillRetry { get; set; }
    public DateTime? NextRetryAt { get; set; }
}

public class WebhookProcessingResult
{
    public string WebhookId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ProcessInstanceId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ExtractedVariables { get; set; } = new();
}

#endregion
