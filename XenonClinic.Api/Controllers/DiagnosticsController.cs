using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for application diagnostics and metrics.
/// Provides runtime statistics, performance metrics, and system health information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiagnosticsController : BaseApiController
{
    private static readonly DateTime _applicationStartTime = DateTime.UtcNow;
    private static long _totalRequests;
    private readonly ClinicDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        ClinicDbContext context,
        ICacheService cacheService,
        ILogger<DiagnosticsController> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get application runtime metrics.
    /// Returns memory usage, GC statistics, thread counts, and uptime.
    /// </summary>
    [HttpGet("metrics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ApplicationMetrics>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<ApplicationMetrics>> GetMetrics()
    {
        Interlocked.Increment(ref _totalRequests);

        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - _applicationStartTime;

        var metrics = new ApplicationMetrics
        {
            Timestamp = DateTime.UtcNow,
            ApplicationName = "XenonClinic",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Uptime = new UptimeInfo
            {
                Days = uptime.Days,
                Hours = uptime.Hours,
                Minutes = uptime.Minutes,
                Seconds = uptime.Seconds,
                TotalSeconds = (long)uptime.TotalSeconds,
                StartTime = _applicationStartTime
            },
            Memory = new MemoryMetrics
            {
                WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                PrivateMemoryMB = process.PrivateMemorySize64 / 1024 / 1024,
                ManagedMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                GcGen0Collections = GC.CollectionCount(0),
                GcGen1Collections = GC.CollectionCount(1),
                GcGen2Collections = GC.CollectionCount(2),
                GcTotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
                GcHeapSizeMB = GC.GetGCMemoryInfo().HeapSizeBytes / 1024 / 1024
            },
            Threads = new ThreadMetrics
            {
                ThreadCount = process.Threads.Count,
                ThreadPoolWorkerThreads = GetThreadPoolStats().workerThreads,
                ThreadPoolCompletionPortThreads = GetThreadPoolStats().completionPortThreads,
                ThreadPoolPendingWorkItems = ThreadPool.PendingWorkItemCount
            },
            Runtime = new RuntimeInfo
            {
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                OsDescription = RuntimeInformation.OSDescription,
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                MachineName = Environment.MachineName
            },
            Requests = new RequestMetrics
            {
                TotalRequests = Interlocked.Read(ref _totalRequests)
            }
        };

        return ApiOk(metrics);
    }

    /// <summary>
    /// Get database connection statistics.
    /// Returns pool stats and connection health.
    /// </summary>
    [HttpGet("metrics/database")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<DatabaseMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDatabaseMetrics()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            stopwatch.Stop();

            var metrics = new DatabaseMetrics
            {
                Timestamp = DateTime.UtcNow,
                IsConnected = canConnect,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Provider = _context.Database.ProviderName ?? "unknown",
                ConnectionString = MaskConnectionString(_context.Database.GetConnectionString() ?? "")
            };

            return ApiOk(metrics);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database metrics check failed");

            return ApiOk(new DatabaseMetrics
            {
                Timestamp = DateTime.UtcNow,
                IsConnected = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get cache statistics.
    /// Returns cache health and operation timing.
    /// </summary>
    [HttpGet("metrics/cache")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<CacheMetrics>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCacheMetrics()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var testKey = $"metrics_test_{Guid.NewGuid():N}";
            var testValue = DateTime.UtcNow.Ticks;

            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(5));
            var retrieved = await _cacheService.GetAsync<long>(testKey);
            await _cacheService.RemoveAsync(testKey);

            stopwatch.Stop();

            var metrics = new CacheMetrics
            {
                Timestamp = DateTime.UtcNow,
                IsOperational = retrieved == testValue,
                RoundTripMs = stopwatch.ElapsedMilliseconds,
                ReadWriteMatch = retrieved == testValue
            };

            return ApiOk(metrics);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Cache metrics check failed");

            return ApiOk(new CacheMetrics
            {
                Timestamp = DateTime.UtcNow,
                IsOperational = false,
                RoundTripMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Increment the request counter (used by middleware).
    /// </summary>
    public static void IncrementRequestCount()
    {
        Interlocked.Increment(ref _totalRequests);
    }

    private static (int workerThreads, int completionPortThreads) GetThreadPoolStats()
    {
        ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
        return (maxWorkerThreads - workerThreads, maxCompletionPortThreads - completionPortThreads);
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "not available";

        // Mask sensitive parts of connection string
        var masked = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(Password|Pwd)=[^;]*",
            "$1=*****",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        masked = System.Text.RegularExpressions.Regex.Replace(
            masked,
            @"(User Id|Uid)=[^;]*",
            "$1=*****",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return masked;
    }
}

#region Metrics DTOs

/// <summary>
/// Complete application metrics snapshot.
/// </summary>
public class ApplicationMetrics
{
    public DateTime Timestamp { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public UptimeInfo Uptime { get; set; } = new();
    public MemoryMetrics Memory { get; set; } = new();
    public ThreadMetrics Threads { get; set; } = new();
    public RuntimeInfo Runtime { get; set; } = new();
    public RequestMetrics Requests { get; set; } = new();
}

/// <summary>
/// Application uptime information.
/// </summary>
public class UptimeInfo
{
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    public long TotalSeconds { get; set; }
    public DateTime StartTime { get; set; }
}

/// <summary>
/// Memory and GC metrics.
/// </summary>
public class MemoryMetrics
{
    public long WorkingSetMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public long ManagedMemoryMB { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }
    public long GcTotalMemoryMB { get; set; }
    public long GcHeapSizeMB { get; set; }
}

/// <summary>
/// Thread pool and thread metrics.
/// </summary>
public class ThreadMetrics
{
    public int ThreadCount { get; set; }
    public int ThreadPoolWorkerThreads { get; set; }
    public int ThreadPoolCompletionPortThreads { get; set; }
    public long ThreadPoolPendingWorkItems { get; set; }
}

/// <summary>
/// Runtime environment information.
/// </summary>
public class RuntimeInfo
{
    public string FrameworkDescription { get; set; } = string.Empty;
    public string OsDescription { get; set; } = string.Empty;
    public string ProcessArchitecture { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public string MachineName { get; set; } = string.Empty;
}

/// <summary>
/// Request statistics.
/// </summary>
public class RequestMetrics
{
    public long TotalRequests { get; set; }
}

/// <summary>
/// Database connection metrics.
/// </summary>
public class DatabaseMetrics
{
    public DateTime Timestamp { get; set; }
    public bool IsConnected { get; set; }
    public long ResponseTimeMs { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string? Error { get; set; }
}

/// <summary>
/// Cache service metrics.
/// </summary>
public class CacheMetrics
{
    public DateTime Timestamp { get; set; }
    public bool IsOperational { get; set; }
    public long RoundTripMs { get; set; }
    public bool ReadWriteMatch { get; set; }
    public string? Error { get; set; }
}

#endregion
