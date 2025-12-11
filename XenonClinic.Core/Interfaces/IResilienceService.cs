namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Resilience service for circuit breaker and rate limiting
/// </summary>
public interface IResilienceService
{
    #region Circuit Breaker

    /// <summary>
    /// Execute an action with circuit breaker protection
    /// </summary>
    Task<T> ExecuteWithCircuitBreakerAsync<T>(string serviceName, Func<Task<T>> action);

    /// <summary>
    /// Execute an action with circuit breaker protection (void)
    /// </summary>
    Task ExecuteWithCircuitBreakerAsync(string serviceName, Func<Task> action);

    /// <summary>
    /// Get circuit breaker state for a service
    /// </summary>
    CircuitBreakerState GetCircuitBreakerState(string serviceName);

    /// <summary>
    /// Reset circuit breaker for a service
    /// </summary>
    void ResetCircuitBreaker(string serviceName);

    /// <summary>
    /// Get all circuit breaker statuses
    /// </summary>
    Dictionary<string, CircuitBreakerState> GetAllCircuitBreakerStates();

    #endregion

    #region Rate Limiting

    /// <summary>
    /// Check if request is allowed (rate limiting)
    /// </summary>
    Task<RateLimitResult> CheckRateLimitAsync(string key, int maxRequests, TimeSpan window);

    /// <summary>
    /// Check rate limit and throw if exceeded
    /// </summary>
    Task EnsureRateLimitAsync(string key, int maxRequests, TimeSpan window);

    /// <summary>
    /// Get current rate limit status
    /// </summary>
    Task<RateLimitStatus> GetRateLimitStatusAsync(string key);

    /// <summary>
    /// Reset rate limit for a key
    /// </summary>
    Task ResetRateLimitAsync(string key);

    #endregion

    #region HTTP Client

    /// <summary>
    /// Create a resilient HTTP client with circuit breaker and retry
    /// </summary>
    HttpClient CreateResilientHttpClient(string serviceName, HttpClientOptions? options = null);

    /// <summary>
    /// Execute HTTP request with resilience policies
    /// </summary>
    Task<HttpResponseMessage> ExecuteHttpRequestAsync(
        string serviceName,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        HttpClientOptions? options = null);

    #endregion
}

/// <summary>
/// Circuit breaker state
/// </summary>
public class CircuitBreakerState
{
    public string ServiceName { get; set; } = string.Empty;
    public CircuitState State { get; set; }
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }
    public DateTime? LastFailureTime { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? NextRetryTime { get; set; }
    public string? LastErrorMessage { get; set; }
}

/// <summary>
/// Circuit breaker states
/// </summary>
public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

/// <summary>
/// Rate limit result
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int CurrentCount { get; set; }
    public int MaxRequests { get; set; }
    public TimeSpan WindowSize { get; set; }
    public TimeSpan? RetryAfter { get; set; }
    public int RemainingRequests { get; set; }
}

/// <summary>
/// Rate limit status
/// </summary>
public class RateLimitStatus
{
    public string Key { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public bool IsLimited { get; set; }
}

/// <summary>
/// HTTP client configuration options
/// </summary>
public class HttpClientOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public bool UseExponentialBackoff { get; set; } = true;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, string>? DefaultHeaders { get; set; }
}
