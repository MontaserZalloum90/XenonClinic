using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// API versioning configuration for backward compatibility.
/// </summary>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Adds API versioning support to the service collection.
    /// </summary>
    public static IServiceCollection AddXenonApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default to version 1.0 if not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            // Support multiple versioning methods
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/patients
                new HeaderApiVersionReader("X-Api-Version"), // X-Api-Version: 1.0
                new QueryStringApiVersionReader("api-version") // ?api-version=1.0
            );
        });

        services.AddVersionedApiExplorer(options =>
        {
            // Format version as 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

/// <summary>
/// API version constants for use in controllers.
/// </summary>
public static class ApiVersions
{
    public const string V1 = "1.0";
    public const string V2 = "2.0";

    public static class Routes
    {
        public const string V1Base = "api/v{version:apiVersion}";
        public const string V1 = "api/v1";
        public const string V2 = "api/v2";
    }
}
