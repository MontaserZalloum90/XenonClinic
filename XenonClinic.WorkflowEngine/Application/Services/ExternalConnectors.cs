using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// REST API connector implementation.
/// </summary>
public class RestConnector : IRestConnector
{
    private readonly RestConnectorConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestConnector> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public string ConnectorType => "REST";
    public string ConnectorId => _config.ConnectorId;

    public RestConnector(RestConnectorConfiguration config, HttpClient httpClient, ILogger<RestConnector> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_config.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        _httpClient.Timeout = _config.DefaultTimeout;

        foreach (var header in _config.DefaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("", cancellationToken);
            return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed for connector {ConnectorId}", ConnectorId);
            return false;
        }
    }

    public async Task<ConnectorResponse> ExecuteAsync(ConnectorRequest request, CancellationToken cancellationToken = default)
    {
        var method = request.Operation.ToUpperInvariant() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            _ => HttpMethod.Get
        };

        var endpoint = request.Parameters.TryGetValue("endpoint", out var ep) ? ep?.ToString() ?? "" : "";

        return await ExecuteHttpRequestAsync(method, endpoint, request.Body, request.Headers, cancellationToken);
    }

    public Task<ConnectorResponse> GetAsync(string endpoint, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
    {
        if (queryParams != null && queryParams.Count > 0)
        {
            var query = string.Join("&", queryParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            endpoint = endpoint.Contains('?') ? $"{endpoint}&{query}" : $"{endpoint}?{query}";
        }
        return ExecuteHttpRequestAsync(HttpMethod.Get, endpoint, null, null, cancellationToken);
    }

    public Task<ConnectorResponse> PostAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        return ExecuteHttpRequestAsync(HttpMethod.Post, endpoint, body, null, cancellationToken);
    }

    public Task<ConnectorResponse> PutAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        return ExecuteHttpRequestAsync(HttpMethod.Put, endpoint, body, null, cancellationToken);
    }

    public Task<ConnectorResponse> PatchAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        return ExecuteHttpRequestAsync(HttpMethod.Patch, endpoint, body, null, cancellationToken);
    }

    public Task<ConnectorResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        return ExecuteHttpRequestAsync(HttpMethod.Delete, endpoint, null, null, cancellationToken);
    }

    private async Task<ConnectorResponse> ExecuteHttpRequestAsync(
        HttpMethod method,
        string endpoint,
        object? body,
        Dictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        var response = new ConnectorResponse { RequestId = Guid.NewGuid().ToString() };
        var sw = Stopwatch.StartNew();
        int retryCount = 0;

        while (retryCount <= _config.DefaultMaxRetries)
        {
            try
            {
                using var request = new HttpRequestMessage(method, endpoint);

                // Add authentication
                await AddAuthenticationAsync(request, cancellationToken);

                // Add custom headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                // Add body
                if (body != null)
                {
                    var json = body is string s ? s : JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                if (_config.EnableLogging)
                {
                    _logger.LogDebug("Executing {Method} {Endpoint}", method, endpoint);
                }

                var httpResponse = await _httpClient.SendAsync(request, cancellationToken);

                response.StatusCode = (int)httpResponse.StatusCode;
                response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;
                response.Success = httpResponse.IsSuccessStatusCode;
                response.RawResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                response.RetryCount = retryCount;

                foreach (var header in httpResponse.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }

                // Try to deserialize JSON response
                if (!string.IsNullOrEmpty(response.RawResponse))
                {
                    try
                    {
                        response.Data = JsonSerializer.Deserialize<JsonElement>(response.RawResponse);
                    }
                    catch
                    {
                        response.Data = response.RawResponse;
                    }
                }

                sw.Stop();
                response.ExecutionTime = sw.Elapsed;

                if (!response.Success && IsRetryableStatusCode(httpResponse.StatusCode) && retryCount < _config.DefaultMaxRetries)
                {
                    retryCount++;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    _logger.LogWarning("Retrying request after {Delay}ms (attempt {Retry}/{MaxRetries})",
                        delay.TotalMilliseconds, retryCount, _config.DefaultMaxRetries);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                return response;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                response.Success = false;
                response.ErrorCode = "TIMEOUT";
                response.ErrorMessage = "Request timed out";

                if (retryCount < _config.DefaultMaxRetries)
                {
                    retryCount++;
                    continue;
                }
            }
            catch (HttpRequestException ex)
            {
                response.Success = false;
                response.ErrorCode = "CONNECTION_ERROR";
                response.ErrorMessage = ex.Message;

                if (retryCount < _config.DefaultMaxRetries)
                {
                    retryCount++;
                    continue;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorCode = "UNKNOWN_ERROR";
                response.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Unexpected error executing request");
            }

            break;
        }

        sw.Stop();
        response.ExecutionTime = sw.Elapsed;
        response.RetryCount = retryCount;
        return response;
    }

    private async Task AddAuthenticationAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_config.Authentication == null || _config.Authentication.Type == "None")
            return;

        switch (_config.Authentication.Type)
        {
            case "Basic":
                var basicCredentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_config.Authentication.Username}:{_config.Authentication.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);
                break;

            case "Bearer":
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.Authentication.Token);
                break;

            case "ApiKey":
                request.Headers.TryAddWithoutValidation(
                    _config.Authentication.ApiKeyHeader ?? "X-Api-Key",
                    _config.Authentication.ApiKey);
                break;

            case "OAuth2":
                var token = await GetOAuth2TokenAsync(cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;
        }
    }

    private async Task<string> GetOAuth2TokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var oauth2Config = _config.Authentication?.OAuth2;
        if (oauth2Config == null)
            throw new InvalidOperationException("OAuth2 configuration is missing");

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, oauth2Config.TokenEndpoint);
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = oauth2Config.GrantType,
            ["client_id"] = oauth2Config.ClientId,
            ["client_secret"] = oauth2Config.ClientSecret
        };

        if (!string.IsNullOrEmpty(oauth2Config.Scope))
            formData["scope"] = oauth2Config.Scope;

        if (!string.IsNullOrEmpty(oauth2Config.Resource))
            formData["resource"] = oauth2Config.Resource;

        tokenRequest.Content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.SendAsync(tokenRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to obtain OAuth2 token: {content}");
        }

        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
        _cachedToken = tokenResponse.GetProperty("access_token").GetString()!;

        if (tokenResponse.TryGetProperty("expires_in", out var expiresIn))
        {
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn.GetInt32() - 60); // 60 second buffer
        }
        else
        {
            _tokenExpiry = DateTime.UtcNow.AddMinutes(55); // Default 55 minutes
        }

        return _cachedToken;
    }

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout ||
               statusCode == HttpStatusCode.TooManyRequests ||
               statusCode == HttpStatusCode.InternalServerError ||
               statusCode == HttpStatusCode.BadGateway ||
               statusCode == HttpStatusCode.ServiceUnavailable ||
               statusCode == HttpStatusCode.GatewayTimeout;
    }
}

/// <summary>
/// SOAP web service connector implementation.
/// </summary>
public class SoapConnector : ISoapConnector
{
    private readonly SoapConnectorConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SoapConnector> _logger;

    public string ConnectorType => "SOAP";
    public string ConnectorId => _config.ConnectorId;

    public SoapConnector(SoapConnectorConfiguration config, HttpClient httpClient, ILogger<SoapConnector> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.Timeout = _config.DefaultTimeout;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var wsdl = await GetWsdlAsync(cancellationToken);
            return !string.IsNullOrEmpty(wsdl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SOAP connection test failed for connector {ConnectorId}", ConnectorId);
            return false;
        }
    }

    public async Task<ConnectorResponse> ExecuteAsync(ConnectorRequest request, CancellationToken cancellationToken = default)
    {
        var action = request.Operation;
        var envelope = request.Body?.ToString() ?? "";
        return await InvokeAsync(action, envelope, cancellationToken);
    }

    public async Task<ConnectorResponse> InvokeAsync(string action, string soapEnvelope, CancellationToken cancellationToken = default)
    {
        var response = new ConnectorResponse { RequestId = Guid.NewGuid().ToString() };
        var sw = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.EndpointUrl);

            // Set content type based on SOAP version
            var contentType = _config.SoapVersion == "1.2"
                ? "application/soap+xml; charset=utf-8"
                : "text/xml; charset=utf-8";

            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, contentType);

            // Add SOAPAction header for SOAP 1.1
            if (_config.SoapVersion == "1.1")
            {
                request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{action}\"");
            }

            // Add authentication
            AddAuthentication(request);

            if (_config.EnableLogging)
            {
                _logger.LogDebug("Invoking SOAP action {Action} on {Endpoint}", action, _config.EndpointUrl);
            }

            var httpResponse = await _httpClient.SendAsync(request, cancellationToken);

            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;
            response.RawResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            // Check for SOAP fault
            if (response.RawResponse.Contains("<soap:Fault>", StringComparison.OrdinalIgnoreCase) ||
                response.RawResponse.Contains("<Fault>", StringComparison.OrdinalIgnoreCase))
            {
                response.Success = false;
                response.ErrorCode = "SOAP_FAULT";
                response.ErrorMessage = ExtractSoapFaultMessage(response.RawResponse);
            }
            else
            {
                response.Success = httpResponse.IsSuccessStatusCode;
                response.Data = ParseSoapResponse(response.RawResponse);
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorCode = "SOAP_ERROR";
            response.ErrorMessage = ex.Message;
            _logger.LogError(ex, "SOAP invocation failed for action {Action}", action);
        }

        sw.Stop();
        response.ExecutionTime = sw.Elapsed;
        return response;
    }

    public async Task<string> GetWsdlAsync(CancellationToken cancellationToken = default)
    {
        var wsdlUrl = _config.WsdlUrl;
        if (!wsdlUrl.Contains("?wsdl", StringComparison.OrdinalIgnoreCase) &&
            !wsdlUrl.Contains("?WSDL", StringComparison.OrdinalIgnoreCase))
        {
            wsdlUrl = wsdlUrl.Contains('?') ? $"{wsdlUrl}&wsdl" : $"{wsdlUrl}?wsdl";
        }

        var response = await _httpClient.GetStringAsync(wsdlUrl, cancellationToken);
        return response;
    }

    private void AddAuthentication(HttpRequestMessage request)
    {
        if (_config.Authentication == null || _config.Authentication.Type == "None")
            return;

        switch (_config.Authentication.Type)
        {
            case "Basic":
                var basicCredentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_config.Authentication.Username}:{_config.Authentication.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);
                break;
        }
    }

    private static string ExtractSoapFaultMessage(string soapResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

            var faultString = doc.SelectSingleNode("//faultstring", nsmgr)?.InnerText ??
                              doc.SelectSingleNode("//soap:Reason/soap:Text", nsmgr)?.InnerText ??
                              doc.SelectSingleNode("//soap12:Reason/soap12:Text", nsmgr)?.InnerText;

            return faultString ?? "Unknown SOAP fault";
        }
        catch
        {
            return "Failed to parse SOAP fault";
        }
    }

    private static object? ParseSoapResponse(string soapResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

            var body = doc.SelectSingleNode("//soap:Body", nsmgr) ??
                       doc.SelectSingleNode("//soap12:Body", nsmgr);

            if (body?.FirstChild != null)
            {
                return body.FirstChild.OuterXml;
            }

            return soapResponse;
        }
        catch
        {
            return soapResponse;
        }
    }
}

/// <summary>
/// In-memory message queue connector for development/testing.
/// For production, implement adapters for RabbitMQ, Azure Service Bus, etc.
/// </summary>
public class InMemoryMessageQueueConnector : IMessageQueueConnector
{
    private readonly MessageQueueConnectorConfiguration _config;
    private readonly ILogger<InMemoryMessageQueueConnector> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<QueueMessage>> _queues = new();
    private readonly ConcurrentDictionary<string, List<QueueSubscription>> _subscriptions = new();
    private readonly object _subscriptionLock = new();

    public string ConnectorType => "MessageQueue";
    public string ConnectorId => _config.ConnectorId;

    public InMemoryMessageQueueConnector(MessageQueueConnectorConfiguration config, ILogger<InMemoryMessageQueueConnector> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true); // In-memory is always available
    }

    public Task<ConnectorResponse> ExecuteAsync(ConnectorRequest request, CancellationToken cancellationToken = default)
    {
        // Generic execute - determine operation from request
        return Task.FromResult(new ConnectorResponse
        {
            RequestId = request.RequestId,
            Success = true,
            StatusCode = 200,
            StatusMessage = "OK"
        });
    }

    public async Task SendMessageAsync(string queueName, object message, MessageProperties? properties = null, CancellationToken cancellationToken = default)
    {
        var queue = _queues.GetOrAdd(queueName, _ => new ConcurrentQueue<QueueMessage>());

        var queueMessage = new QueueMessage
        {
            MessageId = properties?.MessageId ?? Guid.NewGuid().ToString(),
            CorrelationId = properties?.CorrelationId,
            QueueName = queueName,
            Body = message,
            RawBody = message is string s ? s : JsonSerializer.Serialize(message),
            Properties = properties ?? new MessageProperties(),
            ReceivedAt = DateTime.UtcNow,
            DeliveryCount = 0
        };

        queue.Enqueue(queueMessage);

        _logger.LogDebug("Message {MessageId} sent to queue {QueueName}", queueMessage.MessageId, queueName);

        // Notify subscribers
        await NotifySubscribersAsync(queueName, queueMessage, cancellationToken);
    }

    public Task<QueueMessage?> ReceiveMessageAsync(string queueName, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var queue = _queues.GetOrAdd(queueName, _ => new ConcurrentQueue<QueueMessage>());

        if (queue.TryDequeue(out var message))
        {
            message.DeliveryCount++;
            _logger.LogDebug("Message {MessageId} received from queue {QueueName}", message.MessageId, queueName);
            return Task.FromResult<QueueMessage?>(message);
        }

        return Task.FromResult<QueueMessage?>(null);
    }

    public Task AcknowledgeMessageAsync(string queueName, string messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Message {MessageId} acknowledged on queue {QueueName}", messageId, queueName);
        return Task.CompletedTask;
    }

    public Task RejectMessageAsync(string queueName, string messageId, bool requeue = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Message {MessageId} rejected on queue {QueueName}, requeue={Requeue}", messageId, queueName, requeue);
        return Task.CompletedTask;
    }

    public IDisposable Subscribe(string queueName, Func<QueueMessage, CancellationToken, Task<bool>> handler)
    {
        var subscription = new QueueSubscription
        {
            Id = Guid.NewGuid().ToString(),
            QueueName = queueName,
            Handler = handler
        };

        lock (_subscriptionLock)
        {
            if (!_subscriptions.TryGetValue(queueName, out var subs))
            {
                subs = new List<QueueSubscription>();
                _subscriptions[queueName] = subs;
            }
            subs.Add(subscription);
        }

        _logger.LogDebug("Subscription {SubscriptionId} added for queue {QueueName}", subscription.Id, queueName);

        return new EventSubscription(() =>
        {
            lock (_subscriptionLock)
            {
                if (_subscriptions.TryGetValue(queueName, out var subs))
                {
                    subs.Remove(subscription);
                }
            }
            _logger.LogDebug("Subscription {SubscriptionId} removed from queue {QueueName}", subscription.Id, queueName);
        });
    }

    private async Task NotifySubscribersAsync(string queueName, QueueMessage message, CancellationToken cancellationToken)
    {
        List<QueueSubscription> subscribers;
        lock (_subscriptionLock)
        {
            if (!_subscriptions.TryGetValue(queueName, out var subs))
                return;
            subscribers = subs.ToList();
        }

        foreach (var subscriber in subscribers)
        {
            try
            {
                await subscriber.Handler(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscriber {SubscriptionId} failed to handle message {MessageId}",
                    subscriber.Id, message.MessageId);
            }
        }
    }

    private class QueueSubscription
    {
        public string Id { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public Func<QueueMessage, CancellationToken, Task<bool>> Handler { get; set; } = null!;
    }
}

/// <summary>
/// Factory for creating and managing external connectors.
/// </summary>
public class ConnectorFactory : IConnectorFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, IExternalConnector> _connectors = new();

    public ConnectorFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    public IExternalConnector CreateConnector(ConnectorConfiguration config)
    {
        return config switch
        {
            RestConnectorConfiguration restConfig => CreateRestConnector(restConfig),
            SoapConnectorConfiguration soapConfig => CreateSoapConnector(soapConfig),
            MessageQueueConnectorConfiguration mqConfig => CreateMessageQueueConnector(mqConfig),
            _ => throw new ArgumentException($"Unknown connector configuration type: {config.GetType().Name}")
        };
    }

    public IRestConnector CreateRestConnector(RestConnectorConfiguration config)
    {
        var httpClient = _httpClientFactory.CreateClient($"RestConnector_{config.ConnectorId}");
        var logger = _loggerFactory.CreateLogger<RestConnector>();
        var connector = new RestConnector(config, httpClient, logger);
        _connectors[config.ConnectorId] = connector;
        return connector;
    }

    public ISoapConnector CreateSoapConnector(SoapConnectorConfiguration config)
    {
        var httpClient = _httpClientFactory.CreateClient($"SoapConnector_{config.ConnectorId}");
        var logger = _loggerFactory.CreateLogger<SoapConnector>();
        var connector = new SoapConnector(config, httpClient, logger);
        _connectors[config.ConnectorId] = connector;
        return connector;
    }

    public IMessageQueueConnector CreateMessageQueueConnector(MessageQueueConnectorConfiguration config)
    {
        var logger = _loggerFactory.CreateLogger<InMemoryMessageQueueConnector>();
        var connector = new InMemoryMessageQueueConnector(config, logger);
        _connectors[config.ConnectorId] = connector;
        return connector;
    }

    public void RegisterConnector(string connectorId, IExternalConnector connector)
    {
        _connectors[connectorId] = connector;
    }

    public IExternalConnector? GetConnector(string connectorId)
    {
        return _connectors.TryGetValue(connectorId, out var connector) ? connector : null;
    }

    public IEnumerable<string> GetRegisteredConnectorIds()
    {
        return _connectors.Keys;
    }
}
