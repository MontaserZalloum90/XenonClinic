using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// Serilog structured logging configuration.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog with structured logging for the application.
    /// </summary>
    public static IHostBuilder UseXenonLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "XenonClinic")
                .ConfigureForEnvironment(context.HostingEnvironment);
        });
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

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "unknown");
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
