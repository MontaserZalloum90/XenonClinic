using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// CORS configuration options.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS requests.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of allowed HTTP methods.
    /// </summary>
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };

    /// <summary>
    /// List of allowed headers.
    /// </summary>
    public string[] AllowedHeaders { get; set; } = new[] { "Content-Type", "Authorization", "X-Correlation-ID", "X-Tenant-ID" };

    /// <summary>
    /// List of headers exposed to the client.
    /// </summary>
    public string[] ExposedHeaders { get; set; } = new[] { "X-Correlation-ID", "X-Request-ID", "X-Pagination-Total", "X-Pagination-Page" };

    /// <summary>
    /// Whether to allow credentials (cookies, authorization headers).
    /// </summary>
    public bool AllowCredentials { get; set; } = true;

    /// <summary>
    /// How long (in seconds) the preflight request can be cached.
    /// </summary>
    public int PreflightMaxAge { get; set; } = 600; // 10 minutes
}

/// <summary>
/// Extension methods for CORS configuration.
/// </summary>
public static class CorsConfiguration
{
    public const string DefaultPolicyName = "XenonClinicCorsPolicy";
    public const string AllowAllPolicyName = "AllowAll";

    /// <summary>
    /// Adds CORS services with configuration from appsettings.
    /// </summary>
    public static IServiceCollection AddXenonCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
            ?? new CorsOptions();

        services.AddCors(options =>
        {
            // Default policy - restricted to configured origins
            options.AddPolicy(DefaultPolicyName, policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins);
                }
                else
                {
                    // If no origins configured, deny all cross-origin requests
                    policy.SetIsOriginAllowed(_ => false);
                }

                policy.WithMethods(corsOptions.AllowedMethods)
                      .WithHeaders(corsOptions.AllowedHeaders)
                      .WithExposedHeaders(corsOptions.ExposedHeaders)
                      .SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAge));

                if (corsOptions.AllowCredentials && corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.AllowCredentials();
                }
            });

            // Development-only policy - allows any origin
            options.AddPolicy(AllowAllPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders(corsOptions.ExposedHeaders);
            });

            // Strict API policy - minimal headers, no credentials
            options.AddPolicy("StrictApi", policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins);
                }
                else
                {
                    policy.SetIsOriginAllowed(_ => false);
                }

                policy.WithMethods("GET", "POST", "PUT", "DELETE")
                      .WithHeaders("Content-Type", "Authorization")
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        // Register options for injection
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds CORS services for development (allows any origin).
    /// </summary>
    public static IServiceCollection AddXenonCorsDevelopment(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .WithExposedHeaders("X-Correlation-ID", "X-Request-ID", "X-Pagination-Total");
            });
        });

        return services;
    }

    /// <summary>
    /// Uses the default CORS policy.
    /// </summary>
    public static IApplicationBuilder UseXenonCors(this IApplicationBuilder app)
    {
        return app.UseCors(DefaultPolicyName);
    }

    /// <summary>
    /// Uses the specified CORS policy.
    /// </summary>
    public static IApplicationBuilder UseXenonCors(this IApplicationBuilder app, string policyName)
    {
        return app.UseCors(policyName);
    }
}
