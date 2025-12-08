using Prometheus;

namespace XenonClinic.Web.Configuration;

/// <summary>
/// Prometheus metrics configuration for monitoring
/// </summary>
public static class MetricsConfiguration
{
    // Custom application metrics
    private static readonly Counter HttpRequestsTotal = Metrics.CreateCounter(
        "xenonclinic_http_requests_total",
        "Total number of HTTP requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

    private static readonly Histogram HttpRequestDuration = Metrics.CreateHistogram(
        "xenonclinic_http_request_duration_seconds",
        "HTTP request duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "method", "endpoint" },
            Buckets = new[] { 0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
        });

    private static readonly Counter AuthenticationAttempts = Metrics.CreateCounter(
        "xenonclinic_auth_attempts_total",
        "Total authentication attempts",
        new CounterConfiguration
        {
            LabelNames = new[] { "result" } // success, failed, locked_out
        });

    private static readonly Gauge ActiveUsers = Metrics.CreateGauge(
        "xenonclinic_active_users",
        "Number of active users (with valid tokens)");

    private static readonly Counter DatabaseQueries = Metrics.CreateCounter(
        "xenonclinic_database_queries_total",
        "Total database queries executed",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation", "entity" }
        });

    private static readonly Counter CacheOperations = Metrics.CreateCounter(
        "xenonclinic_cache_operations_total",
        "Cache operations",
        new CounterConfiguration
        {
            LabelNames = new[] { "operation", "result" } // get/set, hit/miss
        });

    private static readonly Counter AuditEvents = Metrics.CreateCounter(
        "xenonclinic_audit_events_total",
        "Audit events logged",
        new CounterConfiguration
        {
            LabelNames = new[] { "action", "entity_type" }
        });

    private static readonly Gauge ModulesEnabled = Metrics.CreateGauge(
        "xenonclinic_modules_enabled",
        "Number of enabled modules");

    /// <summary>
    /// Configure Prometheus metrics services
    /// </summary>
    public static IServiceCollection AddCustomMetrics(this IServiceCollection services)
    {
        // Register the metrics service
        services.AddSingleton<IMetricsService, MetricsService>();

        return services;
    }

    /// <summary>
    /// Configure Prometheus metrics endpoints
    /// </summary>
    public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder app)
    {
        // Use HTTP metrics (built-in)
        app.UseHttpMetrics(options =>
        {
            options.AddCustomLabel("host", context => context.Request.Host.Host);
        });

        // Expose metrics endpoint
        app.UseMetricServer("/metrics");

        return app;
    }

    /// <summary>
    /// Track HTTP request for custom metrics
    /// </summary>
    public static void TrackHttpRequest(string method, string endpoint, int statusCode, double durationSeconds)
    {
        HttpRequestsTotal.WithLabels(method, endpoint, statusCode.ToString()).Inc();
        HttpRequestDuration.WithLabels(method, endpoint).Observe(durationSeconds);
    }

    /// <summary>
    /// Track authentication attempt
    /// </summary>
    public static void TrackAuthAttempt(string result)
    {
        AuthenticationAttempts.WithLabels(result).Inc();
    }

    /// <summary>
    /// Track active users
    /// </summary>
    public static void SetActiveUsers(int count)
    {
        ActiveUsers.Set(count);
    }

    /// <summary>
    /// Track database query
    /// </summary>
    public static void TrackDatabaseQuery(string operation, string entity)
    {
        DatabaseQueries.WithLabels(operation, entity).Inc();
    }

    /// <summary>
    /// Track cache operation
    /// </summary>
    public static void TrackCacheOperation(string operation, bool hit)
    {
        CacheOperations.WithLabels(operation, hit ? "hit" : "miss").Inc();
    }

    /// <summary>
    /// Track audit event
    /// </summary>
    public static void TrackAuditEvent(string action, string? entityType)
    {
        AuditEvents.WithLabels(action, entityType ?? "unknown").Inc();
    }

    /// <summary>
    /// Set enabled modules count
    /// </summary>
    public static void SetModulesEnabled(int count)
    {
        ModulesEnabled.Set(count);
    }
}

/// <summary>
/// Interface for metrics tracking
/// </summary>
public interface IMetricsService
{
    void TrackRequest(string method, string endpoint, int statusCode, double durationSeconds);
    void TrackAuth(string result);
    void TrackDbQuery(string operation, string entity);
    void TrackCache(string operation, bool hit);
    void TrackAudit(string action, string? entityType);
}

/// <summary>
/// Metrics service implementation
/// </summary>
public class MetricsService : IMetricsService
{
    public void TrackRequest(string method, string endpoint, int statusCode, double durationSeconds)
        => MetricsConfiguration.TrackHttpRequest(method, endpoint, statusCode, durationSeconds);

    public void TrackAuth(string result)
        => MetricsConfiguration.TrackAuthAttempt(result);

    public void TrackDbQuery(string operation, string entity)
        => MetricsConfiguration.TrackDatabaseQuery(operation, entity);

    public void TrackCache(string operation, bool hit)
        => MetricsConfiguration.TrackCacheOperation(operation, hit);

    public void TrackAudit(string action, string? entityType)
        => MetricsConfiguration.TrackAuditEvent(action, entityType);
}
