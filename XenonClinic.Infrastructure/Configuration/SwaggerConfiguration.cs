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
                Description = description ?? "XenonClinic Healthcare Management System API",
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

            // Group by controller/tag
            options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

            // Order by tag name
            options.OrderActionsBy(api => api.RelativePath);
        });

        return services;
    }

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
