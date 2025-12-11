namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Executes service tasks (HTTP calls, external service invocations).
/// </summary>
public interface IServiceTaskExecutor
{
    /// <summary>
    /// Executes a service task.
    /// </summary>
    Task<ServiceTaskResult> ExecuteAsync(
        Guid? processInstanceId,
        string? activityInstanceId,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an HTTP service task.
    /// </summary>
    Task<ServiceTaskResult> ExecuteHttpAsync(
        HttpServiceTaskRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a custom service handler.
    /// </summary>
    void RegisterHandler(string serviceName, IServiceHandler handler);
}

/// <summary>
/// Interface for custom service handlers.
/// </summary>
public interface IServiceHandler
{
    /// <summary>
    /// Executes the service with the given parameters.
    /// </summary>
    Task<ServiceTaskResult> ExecuteAsync(
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class HttpServiceTaskRequest
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public object? Body { get; set; }
    public string? ContentType { get; set; } = "application/json";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool FollowRedirects { get; set; } = true;

    /// <summary>
    /// Expected successful status codes (default: 200-299).
    /// </summary>
    public List<int>? SuccessStatusCodes { get; set; }

    /// <summary>
    /// Whether to throw on non-success status codes.
    /// </summary>
    public bool ThrowOnError { get; set; } = true;

    /// <summary>
    /// JSONPath or property path to extract result from response.
    /// </summary>
    public string? ResultPath { get; set; }
}

public class ServiceTaskResult
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? ResponseHeaders { get; set; }
    public TimeSpan Duration { get; set; }

    public static ServiceTaskResult Ok(object? data = null) => new()
    {
        Success = true,
        Data = data
    };

    public static ServiceTaskResult Fail(string error, int? statusCode = null) => new()
    {
        Success = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };
}

#endregion
