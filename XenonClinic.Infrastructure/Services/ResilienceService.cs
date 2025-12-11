using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Circuit breaker states
/// </summary>
public enum CircuitState
{
    Closed,     // Normal operation
    Open,       // Failing, reject requests
    HalfOpen    // Testing if service recovered
}

/// <summary>
/// Circuit breaker implementation for external API calls
/// </summary>
public class CircuitBreaker
{
    private readonly string _serviceName;
    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration;
    private readonly ILogger _logger;
    
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _lastFailureTime;
    private DateTime _openedAt;
    private readonly object _lock = new();

    public CircuitBreaker(string serviceName, int failureThreshold, TimeSpan openDuration, ILogger logger)
    {
        _serviceName = serviceName;
        _failureThreshold = failureThreshold;
        _openDuration = openDuration;
        _logger = logger;
    }

    public CircuitState State => _state;
    public int FailureCount => _failureCount;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Task<T>>? fallback = null)
    {
        if (!CanExecute())
        {
            _logger.LogWarning("Circuit breaker OPEN for {Service}, rejecting request", _serviceName);
            if (fallback != null)
                return await fallback();
            throw new CircuitBreakerOpenException($"Circuit breaker is open for {_serviceName}");
        }

        try
        {
            var result = await action();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            if (fallback != null)
                return await fallback();
            throw;
        }
    }

    public async Task ExecuteAsync(Func<Task> action, Func<Task>? fallback = null)
    {
        await ExecuteAsync(async () => { await action(); return true; }, 
            fallback != null ? async () => { await fallback(); return true; } : null);
    }

    private bool CanExecute()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitState.Closed:
                    return true;
                case CircuitState.Open:
                    if (DateTime.UtcNow - _openedAt >= _openDuration)
                    {
                        _state = CircuitState.HalfOpen;
                        _logger.LogInformation("Circuit breaker {Service} transitioning to HALF-OPEN", _serviceName);
                        return true;
                    }
                    return false;
                case CircuitState.HalfOpen:
                    return true;
                default:
                    return false;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                _logger.LogInformation("Circuit breaker {Service} CLOSED after successful test", _serviceName);
            }
            else if (_state == CircuitState.Closed)
            {
                _failureCount = 0;
            }
        }
    }

    private void OnFailure(Exception ex)
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning("Circuit breaker {Service} re-OPENED after failed test", _serviceName);
            }
            else if (_failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
                _logger.LogWarning("Circuit breaker {Service} OPENED after {Count} failures: {Error}", 
                    _serviceName, _failureCount, ex.Message);
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
            _logger.LogInformation("Circuit breaker {Service} manually reset", _serviceName);
        }
    }
}

/// <summary>
/// Circuit breaker open exception
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}

/// <summary>
/// Rate limiter using sliding window algorithm
/// </summary>
public class RateLimiter
{
    private readonly string _serviceName;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    private readonly ConcurrentQueue<DateTime> _requestTimes = new();
    private readonly object _lock = new();

    public RateLimiter(string serviceName, int maxRequests, TimeSpan window)
    {
        _serviceName = serviceName;
        _maxRequests = maxRequests;
        _window = window;
    }

    public bool TryAcquire()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - _window;

            // Remove old entries
            while (_requestTimes.TryPeek(out var oldest) && oldest < windowStart)
            {
                _requestTimes.TryDequeue(out _);
            }

            if (_requestTimes.Count >= _maxRequests)
            {
                return false;
            }

            _requestTimes.Enqueue(now);
            return true;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (!TryAcquire())
        {
            throw new RateLimitExceededException($"Rate limit exceeded for {_serviceName}");
        }
        return await action();
    }

    public TimeSpan? GetRetryAfter()
    {
        lock (_lock)
        {
            if (_requestTimes.TryPeek(out var oldest))
            {
                var windowEnd = oldest + _window;
                var retryAfter = windowEnd - DateTime.UtcNow;
                return retryAfter > TimeSpan.Zero ? retryAfter : null;
            }
            return null;
        }
    }
}

/// <summary>
/// Rate limit exceeded exception
/// </summary>
public class RateLimitExceededException : Exception
{
    public TimeSpan? RetryAfter { get; }
    
    public RateLimitExceededException(string message, TimeSpan? retryAfter = null) : base(message)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Resilient HTTP client wrapper with circuit breaker and rate limiting
/// </summary>
public class ResilientHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly CircuitBreaker _circuitBreaker;
    private readonly RateLimiter _rateLimiter;
    private readonly ILogger _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    public ResilientHttpClient(
        HttpClient httpClient,
        string serviceName,
        ILogger logger,
        int maxRequestsPerMinute = 60,
        int failureThreshold = 5,
        int maxRetries = 3)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromSeconds(2);
        
        _circuitBreaker = new CircuitBreaker(serviceName, failureThreshold, TimeSpan.FromMinutes(1), logger);
        _rateLimiter = new RateLimiter(serviceName, maxRequestsPerMinute, TimeSpan.FromMinutes(1));
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await ExecuteWithResilienceAsync(() => _httpClient.GetAsync(url));
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        return await ExecuteWithResilienceAsync(() => _httpClient.PostAsync(url, content));
    }

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
    {
        return await ExecuteWithResilienceAsync(() => _httpClient.PutAsync(url, content));
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await ExecuteWithResilienceAsync(() => _httpClient.DeleteAsync(url));
    }

    private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(Func<Task<HttpResponseMessage>> action)
    {
        // Check rate limit first
        if (!_rateLimiter.TryAcquire())
        {
            var retryAfter = _rateLimiter.GetRetryAfter();
            throw new RateLimitExceededException("Rate limit exceeded", retryAfter);
        }

        // Execute with circuit breaker and retry logic
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    var response = await action();

                    // Treat server errors as failures for circuit breaker
                    if ((int)response.StatusCode >= 500)
                    {
                        throw new HttpRequestException($"Server error: {response.StatusCode}");
                    }

                    return response;
                }
                catch (Exception ex) when (attempt < _maxRetries && IsTransientError(ex))
                {
                    lastException = ex;
                    var delay = _retryDelay * (int)Math.Pow(2, attempt); // Exponential backoff
                    _logger.LogWarning("Request failed (attempt {Attempt}/{MaxRetries}), retrying in {Delay}ms: {Error}",
                        attempt + 1, _maxRetries + 1, delay.TotalMilliseconds, ex.Message);
                    await Task.Delay(delay);
                }
            }

            throw lastException ?? new InvalidOperationException("Unexpected error in retry logic");
        });
    }

    private static bool IsTransientError(Exception ex)
    {
        return ex is HttpRequestException || 
               ex is TaskCanceledException || 
               ex is TimeoutException;
    }

    public CircuitState CircuitState => _circuitBreaker.State;
    public void ResetCircuitBreaker() => _circuitBreaker.Reset();
}

/// <summary>
/// Factory for creating resilient HTTP clients
/// </summary>
public interface IResilientHttpClientFactory
{
    ResilientHttpClient CreateClient(string serviceName, int maxRequestsPerMinute = 60);
}

public class ResilientHttpClientFactory : IResilientHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, ResilientHttpClient> _clients = new();

    public ResilientHttpClientFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    public ResilientHttpClient CreateClient(string serviceName, int maxRequestsPerMinute = 60)
    {
        return _clients.GetOrAdd(serviceName, name =>
        {
            var httpClient = _httpClientFactory.CreateClient(name);
            var logger = _loggerFactory.CreateLogger<ResilientHttpClient>();
            return new ResilientHttpClient(httpClient, name, logger, maxRequestsPerMinute);
        });
    }
}
