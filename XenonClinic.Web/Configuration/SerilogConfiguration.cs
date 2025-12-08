using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace XenonClinic.Web.Configuration;

/// <summary>
/// Serilog configuration for structured logging
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configure Serilog for the application
    /// </summary>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        // Build Serilog configuration
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "XenonClinic")
            .Enrich.WithProperty("Version", GetApplicationVersion())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}",
                theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            .WriteTo.File(
                path: "logs/xenonclinic-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: "logs/xenonclinic-json-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);

        // Add Seq sink if configured (centralized logging)
        var seqUrl = configuration["Seq:ServerUrl"];
        var seqApiKey = configuration["Seq:ApiKey"];
        if (!string.IsNullOrEmpty(seqUrl))
        {
            loggerConfig.WriteTo.Seq(
                serverUrl: seqUrl,
                apiKey: seqApiKey,
                restrictedToMinimumLevel: LogEventLevel.Information);

            Console.WriteLine($"[System] Seq centralized logging enabled: {seqUrl}");
        }

        Log.Logger = loggerConfig.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Configure Serilog request logging for the application
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";

            // Attach additional properties to the request completion event
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIP", GetClientIpAddress(httpContext));

                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }

                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                }

                if (httpContext.Items.TryGetValue("TenantId", out var tenantId))
                {
                    diagnosticContext.Set("TenantId", tenantId);
                }

                if (httpContext.Items.TryGetValue("CompanyId", out var companyId))
                {
                    diagnosticContext.Set("CompanyId", companyId);
                }
            };

            // Use custom log level based on status code
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                // Log slow requests as warnings
                if (elapsed > 5000)
                    return LogEventLevel.Warning;

                // Don't log health checks at Information level
                if (httpContext.Request.Path.StartsWithSegments("/health") ||
                    httpContext.Request.Path.StartsWithSegments("/healthz"))
                    return LogEventLevel.Debug;

                return LogEventLevel.Information;
            };
        });
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetApplicationVersion()
    {
        return typeof(SerilogConfiguration).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    }
}

/// <summary>
/// Serilog enrichers for correlation ID and user context
/// </summary>
public static class SerilogEnrichers
{
    /// <summary>
    /// Add correlation ID to all log entries in the current scope
    /// </summary>
    public static IDisposable PushCorrelationId(string correlationId)
    {
        return Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId);
    }

    /// <summary>
    /// Add user context to all log entries in the current scope
    /// </summary>
    public static IDisposable PushUserContext(string? userId, string? userName)
    {
        var disposables = new List<IDisposable>();

        if (!string.IsNullOrEmpty(userId))
            disposables.Add(Serilog.Context.LogContext.PushProperty("UserId", userId));

        if (!string.IsNullOrEmpty(userName))
            disposables.Add(Serilog.Context.LogContext.PushProperty("UserName", userName));

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Add tenant context to all log entries in the current scope
    /// </summary>
    public static IDisposable PushTenantContext(int? tenantId, int? companyId, int? branchId)
    {
        var disposables = new List<IDisposable>();

        if (tenantId.HasValue)
            disposables.Add(Serilog.Context.LogContext.PushProperty("TenantId", tenantId.Value));

        if (companyId.HasValue)
            disposables.Add(Serilog.Context.LogContext.PushProperty("CompanyId", companyId.Value));

        if (branchId.HasValue)
            disposables.Add(Serilog.Context.LogContext.PushProperty("BranchId", branchId.Value));

        return new CompositeDisposable(disposables);
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
