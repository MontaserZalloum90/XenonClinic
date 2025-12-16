using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// OpenTelemetry configuration for distributed tracing and metrics.
/// </summary>
public static class TelemetryConfiguration
{
    private const string ServiceName = "XenonClinic.Api";
    private const string ServiceVersion = "1.0.0";

    /// <summary>
    /// Adds OpenTelemetry tracing, metrics, and logging.
    /// </summary>
    public static IServiceCollection AddXenonTelemetry(
        this IServiceCollection services)
    {
        // Configure resource
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: ServiceName,
                serviceVersion: ServiceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                ["service.namespace"] = "xenonclinic"
            });

        // Configure tracing
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = context =>
                        {
                            // Filter out health checks and static files
                            var path = context.Request.Path.Value ?? "";
                            return !path.Contains("/health") &&
                                   !path.Contains("/swagger") &&
                                   !path.Contains(".js") &&
                                   !path.Contains(".css");
                        };
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                            activity.SetTag("http.user_agent", request.Headers["User-Agent"].ToString());
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSource("XenonClinic.*");

                // Default to console exporter for development
                tracing.AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("XenonClinic.*");

                // Custom metrics
                metrics.AddMeter("XenonClinic.Patients");
                metrics.AddMeter("XenonClinic.Appointments");
                metrics.AddMeter("XenonClinic.Orders");

                metrics.AddConsoleExporter();
            });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry logging integration.
    /// </summary>
    public static ILoggingBuilder AddXenonTelemetryLogging(
        this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(ServiceName, serviceVersion: ServiceVersion));

            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            options.AddConsoleExporter();
        });

        return logging;
    }
}
