using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Base interface for all external system connectors.
/// </summary>
public interface IExternalConnector
{
    string ConnectorType { get; }
    string ConnectorId { get; }
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    Task<ConnectorResponse> ExecuteAsync(ConnectorRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Connector request containing all necessary information for external system calls.
/// </summary>
public class ConnectorRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Operation { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public object? Body { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Response from external connector execution.
/// </summary>
public class ConnectorResponse
{
    public string RequestId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public object? Data { get; set; }
    public string? RawResponse { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// REST API connector interface.
/// </summary>
public interface IRestConnector : IExternalConnector
{
    Task<ConnectorResponse> GetAsync(string endpoint, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> PostAsync(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> PutAsync(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> PatchAsync(string endpoint, object body, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);
}

/// <summary>
/// SOAP web service connector interface.
/// </summary>
public interface ISoapConnector : IExternalConnector
{
    Task<ConnectorResponse> InvokeAsync(string action, string soapEnvelope, CancellationToken cancellationToken = default);
    Task<string> GetWsdlAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Message queue connector interface.
/// </summary>
public interface IMessageQueueConnector : IExternalConnector
{
    Task SendMessageAsync(string queueName, object message, MessageProperties? properties = null, CancellationToken cancellationToken = default);
    Task<QueueMessage?> ReceiveMessageAsync(string queueName, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    Task AcknowledgeMessageAsync(string queueName, string messageId, CancellationToken cancellationToken = default);
    Task RejectMessageAsync(string queueName, string messageId, bool requeue = false, CancellationToken cancellationToken = default);
    IDisposable Subscribe(string queueName, Func<QueueMessage, CancellationToken, Task<bool>> handler);
}

/// <summary>
/// Properties for message queue messages.
/// </summary>
public class MessageProperties
{
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ReplyTo { get; set; }
    public string? ContentType { get; set; } = "application/json";
    public int? Priority { get; set; }
    public TimeSpan? Expiration { get; set; }
    public Dictionary<string, object> CustomHeaders { get; set; } = new();
}

/// <summary>
/// Message received from a queue.
/// </summary>
public class QueueMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public object Body { get; set; } = null!;
    public string? RawBody { get; set; }
    public MessageProperties Properties { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public int DeliveryCount { get; set; }
}

/// <summary>
/// Connector factory for creating and managing connectors.
/// </summary>
public interface IConnectorFactory
{
    IExternalConnector CreateConnector(ConnectorConfiguration config);
    IRestConnector CreateRestConnector(RestConnectorConfiguration config);
    ISoapConnector CreateSoapConnector(SoapConnectorConfiguration config);
    IMessageQueueConnector CreateMessageQueueConnector(MessageQueueConnectorConfiguration config);
    void RegisterConnector(string connectorId, IExternalConnector connector);
    IExternalConnector? GetConnector(string connectorId);
    IEnumerable<string> GetRegisteredConnectorIds();
}

/// <summary>
/// Base connector configuration.
/// </summary>
public abstract class ConnectorConfiguration
{
    public string ConnectorId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int DefaultMaxRetries { get; set; } = 3;
    public AuthenticationConfiguration? Authentication { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// REST connector configuration.
/// </summary>
public class RestConnectorConfiguration : ConnectorConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool ValidateSslCertificate { get; set; } = true;
    public string? ProxyUrl { get; set; }
    public Dictionary<string, string> DefaultQueryParams { get; set; } = new();
}

/// <summary>
/// SOAP connector configuration.
/// </summary>
public class SoapConnectorConfiguration : ConnectorConfiguration
{
    public string WsdlUrl { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string SoapVersion { get; set; } = "1.1"; // 1.1 or 1.2
    public string? Namespace { get; set; }
}

/// <summary>
/// Message queue connector configuration.
/// </summary>
public class MessageQueueConnectorConfiguration : ConnectorConfiguration
{
    public string ProviderType { get; set; } = "RabbitMQ"; // RabbitMQ, AzureServiceBus, AmazonSQS
    public string ConnectionString { get; set; } = string.Empty;
    public string? VirtualHost { get; set; }
    public int PrefetchCount { get; set; } = 10;
    public bool AutoAcknowledge { get; set; } = false;
}

/// <summary>
/// Authentication configuration for connectors.
/// </summary>
public class AuthenticationConfiguration
{
    public string Type { get; set; } = "None"; // None, Basic, Bearer, ApiKey, OAuth2, Certificate
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeyHeader { get; set; } = "X-Api-Key";
    public OAuth2Configuration? OAuth2 { get; set; }
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }
}

/// <summary>
/// OAuth2 configuration.
/// </summary>
public class OAuth2Configuration
{
    public string TokenEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GrantType { get; set; } = "client_credentials";
    public string? Scope { get; set; }
    public string? Resource { get; set; }
}
