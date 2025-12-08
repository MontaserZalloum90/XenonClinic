using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace XenonClinic.Web.Configuration;

/// <summary>
/// API versioning configuration for backward compatibility
/// </summary>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Configure API versioning services
    /// </summary>
    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version when not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            // Support multiple versioning methods
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/...
                new HeaderApiVersionReader("X-Api-Version"), // Header: X-Api-Version: 1.0
                new QueryStringApiVersionReader("api-version") // ?api-version=1.0
            );
        })
        .AddApiExplorer(options =>
        {
            // Format: 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger for multiple API versions
    /// </summary>
    public static IServiceCollection AddVersionedSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "XenonClinic API",
                Version = "v1",
                Description = "XenonClinic Healthcare Management System API - Version 1",
                Contact = new OpenApiContact
                {
                    Name = "XenonClinic Team"
                }
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "XenonClinic API",
                Version = "v2",
                Description = "XenonClinic Healthcare Management System API - Version 2 (Preview)",
                Contact = new OpenApiContact
                {
                    Name = "XenonClinic Team"
                }
            });

            // Add JWT Authentication to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include API version in operation IDs
            options.OperationFilter<ApiVersionOperationFilter>();
        });

        return services;
    }
}

/// <summary>
/// Swagger operation filter to add API version parameter
/// </summary>
public class ApiVersionOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var apiVersion = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionAttribute>()
            .FirstOrDefault();

        if (apiVersion != null)
        {
            operation.Description += $"\n\nAPI Version: {string.Join(", ", apiVersion.Versions)}";
        }
    }
}
