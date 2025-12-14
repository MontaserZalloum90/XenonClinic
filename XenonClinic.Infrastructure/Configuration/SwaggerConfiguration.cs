using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// Swagger/OpenAPI configuration extensions.
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Adds enhanced Swagger documentation with JWT support.
    /// </summary>
    public static IServiceCollection AddXenonSwagger(
        this IServiceCollection services,
        string title = "XenonClinic API",
        string version = "v1",
        string? description = null,
        Assembly? xmlCommentsAssembly = null)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description ?? GetDefaultDescription(),
                Contact = new OpenApiContact
                {
                    Name = "XenonClinic Support",
                    Email = "support@xenonclinic.com",
                    Url = new Uri("https://xenonclinic.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary",
                    Url = new Uri("https://xenonclinic.com/license")
                }
            });

            // JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                              Enter 'Bearer' [space] and then your token in the text input below.
                              Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Include XML comments if assembly provided
            if (xmlCommentsAssembly != null)
            {
                var xmlFile = $"{xmlCommentsAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // Custom operation filters
            options.EnableAnnotations();

            // Group by controller/tag with friendly names
            options.TagActionsBy(api =>
            {
                var controller = api.ActionDescriptor.RouteValues["controller"];
                return new[] { GetFriendlyTagName(controller) };
            });

            // Order by tag name
            options.OrderActionsBy(api => api.RelativePath);

            // Add operation ID based on controller and action
            options.CustomOperationIds(api =>
                $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}");
        });

        return services;
    }

    private static string GetDefaultDescription() => @"# XenonClinic Healthcare Management System API

## Overview
Complete API for managing healthcare clinic operations including patients, appointments,
clinical visits, laboratory, radiology, pharmacy, inventory, HR, and financial operations.

## Authentication
All endpoints (except public ones) require JWT Bearer token authentication:
```
Authorization: Bearer <your-token>
```

## Rate Limiting
| Endpoint Type | Limit |
|---------------|-------|
| Authentication | 10 requests/minute |
| General API | 100 requests/minute |
| Sensitive Operations | 5 requests/minute |

## Response Format
```json
{
  ""success"": true,
  ""data"": { },
  ""message"": ""Optional message"",
  ""timestamp"": ""2025-01-01T00:00:00Z""
}
```

## Error Codes
| Code | Description |
|------|-------------|
| 400 | Validation errors |
| 401 | Authentication required |
| 403 | Insufficient permissions |
| 404 | Resource not found |
| 429 | Rate limit exceeded |
| 500 | Server error |
";

    private static string GetFriendlyTagName(string? controller) => controller switch
    {
        "Patient" => "Patients - Patient management and records",
        "PatientPortal" => "Patient Portal - Self-service patient access",
        "Appointments" => "Appointments - Scheduling and calendar",
        "ClinicalVisits" => "Clinical Visits - Patient encounters",
        "Laboratory" => "Laboratory - Lab orders and results",
        "Radiology" => "Radiology - Imaging studies",
        "Inventory" => "Inventory - Stock and supplies",
        "Financial" => "Financial - Billing and payments",
        "Sales" => "Sales - Point of sale operations",
        "HR" => "HR - Human resources",
        "Backup" => "Backup - Disaster recovery",
        "Audit" => "Audit - Activity logging",
        "Security" => "Security - Access control",
        "Analytics" => "Analytics - Reports and insights",
        "Workflows" => "Workflows - Business processes",
        "Consent" => "Consent - Patient consents",
        "ClinicalDecisionSupport" => "Clinical Decision Support - Alerts and recommendations",
        _ => controller ?? "Other"
    };

    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    public static IApplicationBuilder UseXenonSwagger(
        this IApplicationBuilder app,
        string routePrefix = "swagger",
        string version = "v1")
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = $"{routePrefix}/{{documentName}}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/{routePrefix}/{version}/swagger.json", $"XenonClinic API {version}");
            options.RoutePrefix = routePrefix;

            // UI customization
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.DefaultModelsExpandDepth(-1); // Hide schemas by default
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();

            // Custom CSS (optional)
            options.InjectStylesheet("/swagger-ui/custom.css");
        });

        return app;
    }
}
