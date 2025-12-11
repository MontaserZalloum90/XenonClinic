using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for managing webhooks.
/// </summary>
[ApiController]
[Route("api/workflow/webhooks")]
[Authorize]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    #region Outbound Webhook Subscriptions

    /// <summary>
    /// Registers a new webhook subscription.
    /// </summary>
    [HttpPost("subscriptions")]
    public async Task<ActionResult<WebhookSubscription>> RegisterSubscription(
        [FromBody] RegisterWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var subscription = await _webhookService.RegisterSubscriptionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
    }

    /// <summary>
    /// Gets a webhook subscription by ID.
    /// </summary>
    [HttpGet("subscriptions/{id}")]
    public async Task<ActionResult<WebhookSubscription>> GetSubscription(string id, CancellationToken cancellationToken)
    {
        var subscription = await _webhookService.GetSubscriptionAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        return Ok(subscription);
    }

    /// <summary>
    /// Lists all webhook subscriptions.
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<IList<WebhookSubscription>>> ListSubscriptions(
        [FromQuery] string? tenantId,
        [FromQuery] string? eventType,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _webhookService.ListSubscriptionsAsync(tenantId, eventType, cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Updates a webhook subscription.
    /// </summary>
    [HttpPut("subscriptions/{id}")]
    public async Task<ActionResult<WebhookSubscription>> UpdateSubscription(
        string id,
        [FromBody] UpdateWebhookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _webhookService.UpdateSubscriptionAsync(id, request, cancellationToken);
            return Ok(subscription);
        }
        catch (System.InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a webhook subscription.
    /// </summary>
    [HttpDelete("subscriptions/{id}")]
    public async Task<IActionResult> DeleteSubscription(string id, CancellationToken cancellationToken)
    {
        await _webhookService.DeleteSubscriptionAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets delivery history for a subscription.
    /// </summary>
    [HttpGet("subscriptions/{id}/deliveries")]
    public async Task<ActionResult<IList<WebhookDelivery>>> GetDeliveryHistory(
        string id,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var history = await _webhookService.GetDeliveryHistoryAsync(id, limit, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Retries a failed webhook delivery.
    /// </summary>
    [HttpPost("deliveries/{deliveryId}/retry")]
    public async Task<ActionResult<WebhookDeliveryResult>> RetryDelivery(string deliveryId, CancellationToken cancellationToken)
    {
        var result = await _webhookService.RetryDeliveryAsync(deliveryId, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Inbound Webhooks

    /// <summary>
    /// Registers a new inbound webhook endpoint.
    /// </summary>
    [HttpPost("inbound")]
    public async Task<ActionResult<InboundWebhook>> RegisterInboundWebhook(
        [FromBody] RegisterInboundWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var webhook = await _webhookService.RegisterInboundWebhookAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetInboundWebhook), new { id = webhook.Id }, webhook);
    }

    /// <summary>
    /// Gets an inbound webhook by ID.
    /// </summary>
    [HttpGet("inbound/{id}")]
    public async Task<ActionResult<InboundWebhook>> GetInboundWebhook(string id, CancellationToken cancellationToken)
    {
        var webhook = await _webhookService.GetInboundWebhookAsync(id, cancellationToken);
        if (webhook == null)
            return NotFound();

        return Ok(webhook);
    }

    /// <summary>
    /// Receives an inbound webhook (anonymous endpoint for external systems).
    /// </summary>
    [HttpPost("inbound/{webhookId}/receive")]
    [AllowAnonymous]
    public async Task<ActionResult<WebhookProcessingResult>> ReceiveWebhook(
        string webhookId,
        CancellationToken cancellationToken)
    {
        // Read raw body
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        // Parse JSON if applicable
        Dictionary<string, object>? jsonBody = null;
        if (Request.ContentType?.Contains("application/json") == true && !string.IsNullOrEmpty(rawBody))
        {
            try
            {
                jsonBody = JsonSerializer.Deserialize<Dictionary<string, object>>(rawBody);
            }
            catch
            {
                // Not valid JSON, continue with raw body
            }
        }

        // Extract headers
        var headers = new Dictionary<string, string>();
        foreach (var header in Request.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        // Extract query params
        var queryParams = new Dictionary<string, string>();
        foreach (var param in Request.Query)
        {
            queryParams[param.Key] = param.Value.ToString();
        }

        // Get signature from header
        var signatureHeader = "X-Webhook-Signature";
        var webhook = await _webhookService.GetInboundWebhookAsync(webhookId, cancellationToken);
        if (webhook?.SignatureHeader != null)
        {
            signatureHeader = webhook.SignatureHeader;
        }
        Request.Headers.TryGetValue(signatureHeader, out var signature);

        var payload = new InboundWebhookPayload
        {
            ContentType = Request.ContentType ?? "application/octet-stream",
            RawBody = rawBody,
            JsonBody = jsonBody,
            Headers = headers,
            QueryParams = queryParams,
            Signature = signature.ToString(),
            SourceIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        };

        var result = await _webhookService.ProcessInboundWebhookAsync(webhookId, payload, cancellationToken);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "WEBHOOK_NOT_FOUND" => NotFound(result),
                "WEBHOOK_INACTIVE" => StatusCode(503, result),
                "INVALID_SIGNATURE" => Unauthorized(result),
                _ => BadRequest(result)
            };
        }

        return Ok(result);
    }

    #endregion

    #region Test Endpoints

    /// <summary>
    /// Tests a webhook subscription by sending a test event.
    /// </summary>
    [HttpPost("subscriptions/{id}/test")]
    public async Task<ActionResult<WebhookDeliveryResult>> TestSubscription(string id, CancellationToken cancellationToken)
    {
        var subscription = await _webhookService.GetSubscriptionAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound();

        // Create a test event
        var testEvent = new ProcessStartedEvent
        {
            TenantId = subscription.TenantId,
            ProcessInstanceId = "test-instance-id",
            ProcessDefinitionId = "test-definition-id",
            ProcessDefinitionKey = "test-process",
            BusinessKey = "test-business-key",
            StartedBy = "webhook-test"
        };

        await _webhookService.NotifyWebhooksAsync(testEvent, cancellationToken);

        var deliveries = await _webhookService.GetDeliveryHistoryAsync(id, 1, cancellationToken);
        if (deliveries.Count > 0)
        {
            var delivery = deliveries[0];
            return Ok(new WebhookDeliveryResult
            {
                DeliveryId = delivery.Id,
                Success = delivery.Success,
                StatusCode = delivery.ResponseStatusCode ?? 0,
                ErrorMessage = delivery.ErrorMessage,
                ResponseTime = delivery.ResponseTime
            });
        }

        return Ok(new WebhookDeliveryResult
        {
            Success = false,
            ErrorMessage = "No delivery recorded"
        });
    }

    #endregion
}
