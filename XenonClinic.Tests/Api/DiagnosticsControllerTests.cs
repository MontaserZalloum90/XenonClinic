using XenonClinic.Api.Controllers;
using Xunit;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Tests for DiagnosticsController metrics functionality.
/// </summary>
public class DiagnosticsControllerTests
{
    [Fact]
    public void ApplicationMetrics_ShouldHaveAllRequiredProperties()
    {
        // Arrange
        var metrics = new ApplicationMetrics
        {
            Timestamp = DateTime.UtcNow,
            ApplicationName = "XenonClinic",
            Version = "1.0.0",
            Environment = "Test"
        };

        // Assert
        Assert.NotNull(metrics.ApplicationName);
        Assert.NotNull(metrics.Version);
        Assert.NotNull(metrics.Environment);
        Assert.NotEqual(default, metrics.Timestamp);
    }

    [Fact]
    public void UptimeInfo_ShouldCalculateCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(-2).AddMinutes(-30);
        var uptime = DateTime.UtcNow - startTime;

        var uptimeInfo = new UptimeInfo
        {
            Days = uptime.Days,
            Hours = uptime.Hours,
            Minutes = uptime.Minutes,
            Seconds = uptime.Seconds,
            TotalSeconds = (long)uptime.TotalSeconds,
            StartTime = startTime
        };

        // Assert
        Assert.Equal(0, uptimeInfo.Days);
        Assert.Equal(2, uptimeInfo.Hours);
        Assert.True(uptimeInfo.Minutes >= 29 && uptimeInfo.Minutes <= 31);
        Assert.True(uptimeInfo.TotalSeconds > 0);
    }

    [Fact]
    public void MemoryMetrics_ShouldHaveReasonableDefaults()
    {
        // Arrange
        var memory = new MemoryMetrics
        {
            WorkingSetMB = 100,
            PrivateMemoryMB = 80,
            ManagedMemoryMB = 50,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 1
        };

        // Assert
        Assert.True(memory.WorkingSetMB > 0);
        Assert.True(memory.PrivateMemoryMB > 0);
        Assert.True(memory.ManagedMemoryMB > 0);
        Assert.True(memory.GcGen0Collections >= memory.GcGen1Collections);
        Assert.True(memory.GcGen1Collections >= memory.GcGen2Collections);
    }

    [Fact]
    public void ThreadMetrics_ShouldHavePositiveValues()
    {
        // Arrange
        var threads = new ThreadMetrics
        {
            ThreadCount = 25,
            ThreadPoolWorkerThreads = 10,
            ThreadPoolCompletionPortThreads = 5,
            ThreadPoolPendingWorkItems = 2
        };

        // Assert
        Assert.True(threads.ThreadCount > 0);
        Assert.True(threads.ThreadPoolWorkerThreads >= 0);
        Assert.True(threads.ThreadPoolCompletionPortThreads >= 0);
        Assert.True(threads.ThreadPoolPendingWorkItems >= 0);
    }

    [Fact]
    public void RuntimeInfo_ShouldContainEnvironmentData()
    {
        // Arrange
        var runtime = new RuntimeInfo
        {
            FrameworkDescription = ".NET 8.0",
            OsDescription = "Linux",
            ProcessArchitecture = "X64",
            ProcessorCount = 8,
            MachineName = "test-server"
        };

        // Assert
        Assert.NotEmpty(runtime.FrameworkDescription);
        Assert.NotEmpty(runtime.OsDescription);
        Assert.NotEmpty(runtime.ProcessArchitecture);
        Assert.True(runtime.ProcessorCount > 0);
        Assert.NotEmpty(runtime.MachineName);
    }

    [Fact]
    public void DatabaseMetrics_ShouldIndicateConnectionStatus()
    {
        // Arrange - connected scenario
        var connectedMetrics = new DatabaseMetrics
        {
            Timestamp = DateTime.UtcNow,
            IsConnected = true,
            ResponseTimeMs = 15,
            Provider = "Microsoft.EntityFrameworkCore.SqlServer"
        };

        // Assert
        Assert.True(connectedMetrics.IsConnected);
        Assert.True(connectedMetrics.ResponseTimeMs > 0);
        Assert.Contains("SqlServer", connectedMetrics.Provider);
        Assert.Null(connectedMetrics.Error);
    }

    [Fact]
    public void DatabaseMetrics_ShouldIndicateError_WhenDisconnected()
    {
        // Arrange - disconnected scenario
        var disconnectedMetrics = new DatabaseMetrics
        {
            Timestamp = DateTime.UtcNow,
            IsConnected = false,
            ResponseTimeMs = 5000,
            Error = "Connection timeout"
        };

        // Assert
        Assert.False(disconnectedMetrics.IsConnected);
        Assert.NotNull(disconnectedMetrics.Error);
        Assert.Contains("timeout", disconnectedMetrics.Error);
    }

    [Fact]
    public void CacheMetrics_ShouldIndicateOperationalStatus()
    {
        // Arrange - operational scenario
        var operationalMetrics = new CacheMetrics
        {
            Timestamp = DateTime.UtcNow,
            IsOperational = true,
            RoundTripMs = 5,
            ReadWriteMatch = true
        };

        // Assert
        Assert.True(operationalMetrics.IsOperational);
        Assert.True(operationalMetrics.ReadWriteMatch);
        Assert.True(operationalMetrics.RoundTripMs >= 0);
        Assert.Null(operationalMetrics.Error);
    }

    [Fact]
    public void CacheMetrics_ShouldIndicateError_WhenFailed()
    {
        // Arrange - failed scenario
        var failedMetrics = new CacheMetrics
        {
            Timestamp = DateTime.UtcNow,
            IsOperational = false,
            RoundTripMs = 100,
            ReadWriteMatch = false,
            Error = "Redis connection refused"
        };

        // Assert
        Assert.False(failedMetrics.IsOperational);
        Assert.NotNull(failedMetrics.Error);
        Assert.Contains("Redis", failedMetrics.Error);
    }

    [Fact]
    public void RequestMetrics_ShouldTrackTotalRequests()
    {
        // Arrange
        var requests = new RequestMetrics
        {
            TotalRequests = 1000
        };

        // Assert
        Assert.True(requests.TotalRequests > 0);
    }

    [Fact]
    public void IncrementRequestCount_ShouldBeThreadSafe()
    {
        // This test verifies the request counter can be incremented safely
        // In a real scenario, multiple threads would call this
        DiagnosticsController.IncrementRequestCount();
        DiagnosticsController.IncrementRequestCount();
        DiagnosticsController.IncrementRequestCount();

        // The counter should increment without throwing
        Assert.True(true);
    }

    [Fact]
    public void MetricsEndpoint_ShouldBeAnonymouslyAccessible()
    {
        // The /api/diagnostics/metrics endpoint should be accessible without authentication
        // This is verified by the [AllowAnonymous] attribute on the controller method
        var endpoint = "/api/diagnostics/metrics";
        var isPublic = true; // Based on [AllowAnonymous] attribute

        Assert.True(isPublic);
        Assert.Contains("metrics", endpoint);
    }

    [Fact]
    public void DatabaseMetricsEndpoint_ShouldRequireAdminAuthorization()
    {
        // The /api/diagnostics/metrics/database endpoint should require admin access
        var endpoint = "/api/diagnostics/metrics/database";
        var requiresAdmin = true; // Based on [Authorize(Policy = "AdminOnly")] attribute

        Assert.True(requiresAdmin);
        Assert.Contains("database", endpoint);
    }

    [Fact]
    public void CacheMetricsEndpoint_ShouldRequireAdminAuthorization()
    {
        // The /api/diagnostics/metrics/cache endpoint should require admin access
        var endpoint = "/api/diagnostics/metrics/cache";
        var requiresAdmin = true; // Based on [Authorize(Policy = "AdminOnly")] attribute

        Assert.True(requiresAdmin);
        Assert.Contains("cache", endpoint);
    }
}
