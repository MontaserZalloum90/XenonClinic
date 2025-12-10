using Microsoft.Extensions.Configuration;
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

        // Unit of Work - transaction management across repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Tenant isolation service - runtime validation
        services.AddScoped<ITenantIsolationService, TenantIsolationService>();

        return services;
    }

    /// <summary>
    /// Adds database services with proper configuration including retry policies and connection resilience.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="isDevelopment">Whether running in development mode.</param>
    public static IServiceCollection AddXenonDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        return services.AddClinicDatabase(configuration, isDevelopment);
    }

    /// <summary>
    /// Adds in-memory caching services.
    /// Use AddDistributedCaching for Redis-based caching in production.
    /// </summary>
    public static IServiceCollection AddInMemoryCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();
        return services;
    }

    /// <summary>
    /// Adds distributed caching using Redis.
    /// Requires Redis connection configuration.
    /// </summary>
    public static IServiceCollection AddDistributedCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "XenonClinic_";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Adds health check services with database and API checks.
    /// </summary>
    public static IServiceCollection AddXenonHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api", tags: new[] { "api", "ready" })
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "ready" });

        return services;
    }
}
