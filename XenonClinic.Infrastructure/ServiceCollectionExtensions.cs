using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Health;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services with DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core infrastructure services to the service collection.
    /// These are shared services used across all modules.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // User context - centralized user information access
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // Sequence generator - centralized sequence number generation
        services.AddScoped<ISequenceGenerator, SequenceGenerator>();

        // Repository pattern - generic data access
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>
    /// Adds health check services.
    /// </summary>
    public static IServiceCollection AddXenonHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api", tags: new[] { "api", "ready" })
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "ready" });

        return services;
    }
}
