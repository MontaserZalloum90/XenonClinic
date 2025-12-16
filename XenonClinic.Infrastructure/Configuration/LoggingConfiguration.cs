using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// Serilog structured logging configuration with enhanced enrichers and filtering.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with structured logging for the application.
    /// Includes correlation ID, tenant context, and performance tracking.
    /// </summary>
    public static IHostBuilder UseXenonLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "XenonClinic")
                .Enrich.WithProperty("Version", GetApplicationVersion())
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Filter.ByExcluding(IsHealthCheckRequest)
                .ConfigureForEnvironment(context.HostingEnvironment);
        });
    }

    /// <summary>
    /// Gets the application version from the assembly.
    /// </summary>
    private static string GetApplicationVersion()
    {
        return typeof(LoggingConfiguration).Assembly.GetName().Version?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Filters out health check endpoint requests to reduce log noise.
    /// </summary>
    private static bool IsHealthCheckRequest(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("RequestPath", out var requestPath))
        {
            var path = requestPath.ToString().Trim('"');
            return path.StartsWith("/health") || path.StartsWith("/ready") || path.StartsWith("/live");
        }
        return false;
    }

    /// <summary>
    /// Configures logging based on environment.
    /// </summary>
    private static LoggerConfiguration ConfigureForEnvironment(
        this LoggerConfiguration config,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            config
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/xenon-dev-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}");
        }
        else
        {
            config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: "logs/xenon-.json",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 100_000_000, // 100MB
                    rollOnFileSizeLimit: true);
        }

        return config;
    }

    /// <summary>
    /// Adds request logging middleware.
    /// </summary>
    public static IApplicationBuilder UseXenonRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                if (elapsed > 3000) // Slow requests
                    return LogEventLevel.Warning;

                return LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("RequestContentType", httpContext.Request.ContentType ?? "none");
                diagnosticContext.Set("RequestContentLength", httpContext.Request.ContentLength ?? 0);
                diagnosticContext.Set("ResponseContentType", httpContext.Response.ContentType ?? "none");

                // Correlation ID for distributed tracing
                if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId.ToString());
                }
                else if (httpContext.Items.TryGetValue("CorrelationId", out var itemCorrelationId))
                {
                    diagnosticContext.Set("CorrelationId", itemCorrelationId?.ToString() ?? Guid.NewGuid().ToString());
                }

                // Tenant context for multi-tenancy tracking
                if (httpContext.Items.TryGetValue("TenantId", out var tenantId))
                {
                    diagnosticContext.Set("TenantId", tenantId?.ToString() ?? "unknown");
                }

                // User context
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
                }
            };
        });
    }

    /// <summary>
    /// Creates a bootstrap logger for startup errors.
    /// </summary>
    public static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/startup-.log", rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();
    }
}
