using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Implementation of webhook service for managing inbound and outbound webhooks.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;
    private readonly IProcessExecutionService? _processExecutionService;

    // In-memory storage for demo - replace with database in production
    private readonly ConcurrentDictionary<string, WebhookSubscription> _subscriptions = new();
    private readonly ConcurrentDictionary<string, InboundWebhook> _inboundWebhooks = new();
    private readonly ConcurrentDictionary<string, List<WebhookDelivery>> _deliveryHistory = new();

    public WebhookService(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger,
        IProcessExecutionService? processExecutionService = null)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _processExecutionService = processExecutionService;
    }

    #region Webhook Registration

    public Task<WebhookSubscription> RegisterSubscriptionAsync(RegisterWebhookRequest request, CancellationToken cancellationToken = default)
    {
        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = request.TenantId,
            Name = request.Name,
            Description = request.Description,
            TargetUrl = request.TargetUrl,
            EventTypes = request.EventTypes,
            SecretHash = !string.IsNullOrEmpty(request.Secret) ? HashSecret(request.Secret) : null,
            Authentication = request.Authentication,
            CustomHeaders = request.CustomHeaders,
            Active = request.Active,
            MaxRetries = request.MaxRetries,
            TimeoutSeconds = request.TimeoutSeconds,
            CreatedAt = DateTime.UtcNow
        };

        _subscriptions[subscription.Id] = subscription;

        var eventTypes = subscription.EventTypes ?? Enumerable.Empty<string>();
        _logger.LogInformation("Registered webhook subscription {SubscriptionId} for events {EventTypes}",
            subscription.Id, string.Join(", ", eventTypes));

        return Task.FromResult(subscription);
    }

    public Task<WebhookSubscription> UpdateSubscriptionAsync(string subscriptionId, UpdateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (!_subscriptions.TryGetValue(subscriptionId, out var subscription))
        {
            throw new InvalidOperationException($"Webhook subscription {subscriptionId} not found");
        }

        if (request.Name != null) subscription.Name = request.Name;
        if (request.Description != null) subscription.Description = request.Description;
        if (request.TargetUrl != null) subscription.TargetUrl = request.TargetUrl;
        if (request.EventTypes != null) subscription.EventTypes = request.EventTypes;
        if (request.Secret != null) subscription.SecretHash = HashSecret(request.Secret);
        if (request.Authentication != null) subscription.Authentication = request.Authentication;
        if (request.CustomHeaders != null) subscription.CustomHeaders = request.CustomHeaders;
        if (request.Active.HasValue) subscription.Active = request.Active.Value;

        _logger.LogInformation("Updated webhook subscription {SubscriptionId}", subscriptionId);

        return Task.FromResult(subscription);
    }

    public Task DeleteSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        _subscriptions.TryRemove(subscriptionId, out _);
        _deliveryHistory.TryRemove(subscriptionId, out _);

        _logger.LogInformation("Deleted webhook subscription {SubscriptionId}", subscriptionId);

        return Task.CompletedTask;
    }

    public Task<WebhookSubscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        _subscriptions.TryGetValue(subscriptionId, out var subscription);
        return Task.FromResult(subscription);
    }

    public Task<IList<WebhookSubscription>> ListSubscriptionsAsync(string? tenantId = null, string? eventType = null, CancellationToken cancellationToken = default)
    {
        var query = _subscriptions.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(tenantId))
        {
            query = query.Where(s => s.TenantId == tenantId);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(s => s.EventTypes != null && s.EventTypes.Any(et => MatchesEventType(eventType, et)));
        }

        return Task.FromResult<IList<WebhookSubscription>>(query.ToList());
    }

    #endregion

    #region Inbound Webhooks

    public Task<InboundWebhook> RegisterInboundWebhookAsync(RegisterInboundWebhookRequest request, CancellationToken cancellationToken = default)
    {
        var webhook = new InboundWebhook
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = request.TenantId,
            Name = request.Name,
            Description = request.Description,
            SourceSystem = request.SourceSystem,
            Endpoint = $"/api/workflow/webhooks/inbound/{Guid.NewGuid():N}",
            SecretHash = !string.IsNullOrEmpty(request.Secret) ? HashSecret(request.Secret) : null,
            SignatureHeader = request.SignatureHeader,
            SignatureAlgorithm = request.SignatureAlgorithm,
            Action = request.Action,
            PayloadMapping = request.PayloadMapping,
            Active = request.Active,
            CreatedAt = DateTime.UtcNow
        };

        _inboundWebhooks[webhook.Id] = webhook;

        _logger.LogInformation("Registered inbound webhook {WebhookId} from {SourceSystem} at {Endpoint}",
            webhook.Id, webhook.SourceSystem, webhook.Endpoint);

        return Task.FromResult(webhook);
    }

    public async Task<WebhookProcessingResult> ProcessInboundWebhookAsync(string webhookId, InboundWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        var result = new WebhookProcessingResult { WebhookId = webhookId };

        if (!_inboundWebhooks.TryGetValue(webhookId, out var webhook))
        {
            result.Success = false;
            result.ErrorCode = "WEBHOOK_NOT_FOUND";
            result.ErrorMessage = $"Webhook {webhookId} not found";
            return result;
        }

        if (!webhook.Active)
        {
            result.Success = false;
            result.ErrorCode = "WEBHOOK_INACTIVE";
            result.ErrorMessage = "Webhook is inactive";
            return result;
        }

        webhook.ReceivedCount++;

        // Validate signature if configured
        if (!string.IsNullOrEmpty(webhook.SecretHash) && !string.IsNullOrEmpty(payload.Signature))
        {
            var isValid = await ValidateSignatureAsync(webhookId, payload.Signature, payload.RawBody ?? "", cancellationToken);
            if (!isValid)
            {
                webhook.FailedCount++;
                result.Success = false;
                result.ErrorCode = "INVALID_SIGNATURE";
                result.ErrorMessage = "Webhook signature validation failed";
                return result;
            }
        }

        try
        {
            // Extract variables from payload
            var variables = ExtractVariables(payload, webhook.PayloadMapping);
            result.ExtractedVariables = variables;

            // Execute the configured action
            switch (webhook.Action.Type)
            {
                case "StartProcess":
                    if (_processExecutionService != null && !string.IsNullOrEmpty(webhook.Action.ProcessDefinitionKey))
                    {
                        var request = new StartProcessRequest
                        {
                            ProcessKey = webhook.Action.ProcessDefinitionKey,
                            Variables = variables
                        };

                        // Parse tenantId - webhook system uses string tenantIds, but process execution needs int
                        if (!int.TryParse(webhook.TenantId, out var tenantId))
                        {
                            _logger.LogWarning("Invalid tenantId '{TenantId}' for webhook {WebhookId}, using default tenant 1",
                                webhook.TenantId, webhookId);
                            tenantId = 1;
                        }

                        var instance = await _processExecutionService.StartProcessAsync(
                            request,
                            tenantId,
                            "webhook-system",
                            cancellationToken);
                        result.ProcessInstanceId = instance.Id.ToString();
                    }
                    break;

                case "SendMessage":
                    // Would integrate with message correlation service
                    _logger.LogInformation("Would send message {MessageName} with variables", webhook.Action.MessageName);
                    break;

                case "SendSignal":
                    // Would integrate with signal service
                    _logger.LogInformation("Would send signal {SignalName}", webhook.Action.SignalName);
                    break;

                case "TriggerEvent":
                    // Would integrate with event bus
                    _logger.LogInformation("Would trigger event for process {ProcessInstanceId}", webhook.Action.ProcessInstanceId);
                    break;
            }

            webhook.ProcessedCount++;
            result.Success = true;

            _logger.LogInformation("Processed inbound webhook {WebhookId}, action: {ActionType}",
                webhookId, webhook.Action.Type);
        }
        catch (Exception ex)
        {
            webhook.FailedCount++;
            result.Success = false;
            result.ErrorCode = "PROCESSING_ERROR";
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Failed to process inbound webhook {WebhookId}", webhookId);
        }

        return result;
    }

    public Task<InboundWebhook?> GetInboundWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        _inboundWebhooks.TryGetValue(webhookId, out var webhook);
        return Task.FromResult(webhook);
    }

    public Task<bool> ValidateSignatureAsync(string webhookId, string signature, string payload, CancellationToken cancellationToken = default)
    {
        if (!_inboundWebhooks.TryGetValue(webhookId, out var webhook) || string.IsNullOrEmpty(webhook.SecretHash))
        {
            return Task.FromResult(false);
        }

        // For HMAC validation, we need the original secret, not the hash
        // In production, store the secret securely (e.g., Azure Key Vault)
        // For now, we'll do a simple comparison assuming the signature is the expected format

        var algorithm = webhook.SignatureAlgorithm?.ToUpperInvariant() ?? "HMACSHA256";

        try
        {
            if (string.IsNullOrEmpty(signature))
            {
                return Task.FromResult(false);
            }

            // Parse signature format (e.g., "sha256=abc123" or just "abc123")
            var signatureValue = signature;
            var equalsIndex = signature.IndexOf('=');
            if (equalsIndex >= 0 && equalsIndex < signature.Length - 1)
            {
                signatureValue = signature.Substring(equalsIndex + 1);
            }

            // In production, compute HMAC and compare
            // For demo, we accept if signature is present
            var isValid = !string.IsNullOrEmpty(signatureValue);

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating webhook signature for {WebhookId}", webhookId);
            return Task.FromResult(false);
        }
    }

    #endregion

    #region Outbound Webhooks

    public async Task NotifyWebhooksAsync(WorkflowEvent @event, CancellationToken cancellationToken = default)
    {
        var matchingSubscriptions = _subscriptions.Values
            .Where(s => s.Active && s.EventTypes != null && s.EventTypes.Any(et => MatchesEventType(@event.EventType, et)))
            .ToList();

        if (matchingSubscriptions.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Notifying {Count} webhook subscriptions for event {EventType}",
            matchingSubscriptions.Count, @event.EventType);

        var tasks = matchingSubscriptions.Select(s => DeliverWebhookAsync(s, @event, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task<WebhookDeliveryResult> RetryDeliveryAsync(string deliveryId, CancellationToken cancellationToken = default)
    {
        // Find the failed delivery
        WebhookDelivery? delivery = null;
        string? subscriptionId = null;

        // Take a snapshot to avoid concurrent modification
        foreach (var kvp in _deliveryHistory.ToArray())
        {
            List<WebhookDelivery> historySnapshot;
            lock (kvp.Value)
            {
                historySnapshot = kvp.Value.ToList();
            }
            delivery = historySnapshot.FirstOrDefault(d => d.Id == deliveryId);
            if (delivery != null)
            {
                subscriptionId = kvp.Key;
                break;
            }
        }

        if (delivery == null || subscriptionId == null)
        {
            return new WebhookDeliveryResult
            {
                DeliveryId = deliveryId,
                Success = false,
                ErrorMessage = "Delivery not found"
            };
        }

        if (!_subscriptions.TryGetValue(subscriptionId, out var subscription))
        {
            return new WebhookDeliveryResult
            {
                DeliveryId = deliveryId,
                Success = false,
                ErrorMessage = "Subscription not found"
            };
        }

        // Retry the delivery
        return await SendWebhookAsync(subscription, delivery.EventId, delivery.EventType, delivery.RequestBody, delivery.AttemptNumber + 1, cancellationToken);
    }

    public Task<IList<WebhookDelivery>> GetDeliveryHistoryAsync(string subscriptionId, int limit = 50, CancellationToken cancellationToken = default)
    {
        if (_deliveryHistory.TryGetValue(subscriptionId, out var history))
        {
            return Task.FromResult<IList<WebhookDelivery>>(
                history.OrderByDescending(d => d.AttemptedAt).Take(limit).ToList());
        }

        return Task.FromResult<IList<WebhookDelivery>>(new List<WebhookDelivery>());
    }

    private async Task DeliverWebhookAsync(WebhookSubscription subscription, WorkflowEvent @event, CancellationToken cancellationToken)
    {
        var payload = CreateWebhookPayload(@event, subscription);
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var result = await SendWebhookAsync(subscription, @event.EventId, @event.EventType, payloadJson, 1, cancellationToken);

        if (!result.Success && result.WillRetry)
        {
            // Schedule retries with exponential backoff
            await ScheduleRetriesAsync(subscription, @event.EventId, @event.EventType, payloadJson, cancellationToken);
        }
    }

    private async Task<WebhookDeliveryResult> SendWebhookAsync(
        WebhookSubscription subscription,
        string eventId,
        string eventType,
        string payloadJson,
        int attemptNumber,
        CancellationToken cancellationToken)
    {
        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid().ToString(),
            SubscriptionId = subscription.Id,
            EventId = eventId,
            EventType = eventType,
            TargetUrl = subscription.TargetUrl,
            RequestBody = payloadJson,
            AttemptNumber = attemptNumber,
            AttemptedAt = DateTime.UtcNow
        };

        var result = new WebhookDeliveryResult
        {
            DeliveryId = delivery.Id
        };

        var sw = Stopwatch.StartNew();

        try
        {
            var httpClient = _httpClientFactory.CreateClient("WebhookDelivery");
            httpClient.Timeout = TimeSpan.FromSeconds(subscription.TimeoutSeconds);

            using var request = new HttpRequestMessage(HttpMethod.Post, subscription.TargetUrl);
            request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            // Add custom headers
            if (subscription.CustomHeaders != null)
            {
                foreach (var header in subscription.CustomHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add signature if secret is configured
            if (!string.IsNullOrEmpty(subscription.SecretHash))
            {
                var signature = ComputeSignature(payloadJson, subscription.SecretHash);
                request.Headers.TryAddWithoutValidation("X-Webhook-Signature", $"sha256={signature}");
            }

            // Add authentication
            AddAuthentication(request, subscription.Authentication);

            // Add standard webhook headers
            request.Headers.TryAddWithoutValidation("X-Webhook-Event", eventType);
            request.Headers.TryAddWithoutValidation("X-Webhook-Delivery", delivery.Id);
            request.Headers.TryAddWithoutValidation("X-Webhook-Timestamp", DateTime.UtcNow.ToString("O"));

            var response = await httpClient.SendAsync(request, cancellationToken);

            sw.Stop();
            delivery.ResponseTime = sw.Elapsed;
            delivery.ResponseStatusCode = (int)response.StatusCode;
            delivery.ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            delivery.Success = response.IsSuccessStatusCode;

            result.Success = delivery.Success;
            result.StatusCode = delivery.ResponseStatusCode ?? 0;
            result.ResponseTime = delivery.ResponseTime;

            if (delivery.Success)
            {
                subscription.SuccessfulDeliveries++;
                subscription.LastDeliveryAt = DateTime.UtcNow;
                _logger.LogDebug("Webhook delivered successfully to {Url} for event {EventId}",
                    subscription.TargetUrl, eventId);
            }
            else
            {
                subscription.FailedDeliveries++;
                delivery.ErrorMessage = $"HTTP {delivery.ResponseStatusCode}: {delivery.ResponseBody}";
                result.ErrorMessage = delivery.ErrorMessage;
                result.WillRetry = attemptNumber < subscription.MaxRetries;

                if (result.WillRetry)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attemptNumber) * 10); // 10s, 20s, 40s, etc.
                    result.NextRetryAt = DateTime.UtcNow.Add(delay);
                }

                _logger.LogWarning("Webhook delivery failed to {Url} for event {EventId}: {StatusCode}",
                    subscription.TargetUrl, eventId, delivery.ResponseStatusCode);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            delivery.ResponseTime = sw.Elapsed;
            delivery.Success = false;
            delivery.ErrorMessage = ex.Message;

            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ResponseTime = delivery.ResponseTime;
            result.WillRetry = attemptNumber < subscription.MaxRetries;

            subscription.FailedDeliveries++;

            _logger.LogError(ex, "Webhook delivery exception to {Url} for event {EventId}",
                subscription.TargetUrl, eventId);
        }

        // Record delivery in history
        var history = _deliveryHistory.GetOrAdd(subscription.Id, _ => new List<WebhookDelivery>());
        lock (history)
        {
            history.Add(delivery);
            // Keep only last 100 deliveries per subscription
            while (history.Count > 100)
            {
                history.RemoveAt(0);
            }
        }

        return result;
    }

    private Task ScheduleRetriesAsync(WebhookSubscription subscription, string eventId, string eventType, string payloadJson, CancellationToken cancellationToken)
    {
        // In production, this would use the job processor to schedule retries
        // For now, we'll log the intent
        _logger.LogInformation("Would schedule webhook retry for subscription {SubscriptionId}, event {EventId}",
            subscription.Id, eventId);
        return Task.CompletedTask;
    }

    #endregion

    #region Helper Methods

    private static object CreateWebhookPayload(WorkflowEvent @event, WebhookSubscription subscription)
    {
        return new
        {
            @event.EventId,
            @event.EventType,
            @event.Timestamp,
            @event.TenantId,
            @event.CorrelationId,
            @event.Source,
            @event.Metadata,
            Data = @event // Include the full event as data
        };
    }

    private static bool MatchesEventType(string eventType, string pattern)
    {
        if (pattern == "*" || pattern == eventType)
            return true;

        // Support wildcard patterns like "process.*" or "task.**"
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*\\*", ".*")
                .Replace("\\*", "[^.]*") + "$";
            return Regex.IsMatch(eventType, regexPattern, RegexOptions.IgnoreCase);
        }

        return false;
    }

    private static Dictionary<string, object> ExtractVariables(InboundWebhookPayload payload, Dictionary<string, string>? mapping)
    {
        var variables = new Dictionary<string, object>();

        if (payload.JsonBody == null || mapping == null)
            return variables;

        foreach (var kvp in mapping)
        {
            var variableName = kvp.Key;
            var jsonPath = kvp.Value;

            try
            {
                var value = ExtractJsonValue(payload.JsonBody, jsonPath);
                if (value != null)
                {
                    variables[variableName] = value;
                }
            }
            catch (Exception)
            {
                // Silently skip values that can't be extracted
            }
        }

        return variables;
    }

    private static object? ExtractJsonValue(Dictionary<string, object> json, string path)
    {
        var parts = path.Split('.');
        object? current = json;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(part, out current))
                    return null;
            }
            else if (current is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(part, out var prop))
                {
                    current = prop;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private static string HashSecret(string secret)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeSignature(string payload, string secretHash)
    {
        // In production, use the actual secret for HMAC
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretHash));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void AddAuthentication(HttpRequestMessage request, WebhookAuthConfig? auth)
    {
        if (auth == null || auth.Type == "None")
            return;

        switch (auth.Type)
        {
            case "Basic":
                var basicCredentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{auth.Username}:{auth.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);
                break;

            case "Bearer":
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
                break;

            case "ApiKey":
                request.Headers.TryAddWithoutValidation(auth.ApiKeyHeader ?? "X-Api-Key", auth.ApiKey);
                break;
        }
    }

    #endregion
}
