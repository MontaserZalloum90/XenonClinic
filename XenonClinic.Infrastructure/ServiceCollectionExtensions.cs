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
        // BUG FIX: Register ICurrentUserService as alias to ICurrentUserContext for backwards compatibility
        services.AddScoped<ICurrentUserService>(sp => (ICurrentUserService)sp.GetRequiredService<ICurrentUserContext>());

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
    /// Adds security and compliance services (HIPAA, RBAC, Audit).
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // HIPAA-compliant audit logging
        services.AddScoped<IAuditService, AuditService>();

        // Role-Based Access Control
        services.AddScoped<IRbacService, RbacService>();

        // PHI Encryption Service
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Patient Consent Management
        services.AddScoped<IConsentService, ConsentService>();

        // HttpClient for backup storage operations
        services.AddHttpClient("BackupStorage", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(30); // Long timeout for large file transfers
            client.DefaultRequestHeaders.Add("User-Agent", "XenonClinic-BackupService/1.0");
        });

        // Backup and Disaster Recovery
        services.AddScoped<Services.IBackupService, BackupService>();

        // Security Configuration
        services.AddScoped<Services.ISecurityConfigurationService, SecurityConfigurationService>();

        // Resilience Services (Circuit Breaker, Rate Limiting)
        services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>();

        return services;
    }

    /// <summary>
    /// Adds analytics and business intelligence services.
    /// </summary>
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }

    /// <summary>
    /// Adds patient portal services for self-service patient access.
    /// </summary>
    public static IServiceCollection AddPatientPortalServices(this IServiceCollection services)
    {
        services.AddScoped<IPatientPortalService, PatientPortalService>();
        return services;
    }

    /// <summary>
    /// Adds clinical decision support services.
    /// Provides evidence-based clinical recommendations, alerts, and decision support tools.
    /// </summary>
    public static IServiceCollection AddClinicalDecisionSupportServices(this IServiceCollection services)
    {
        services.AddScoped<IClinicalDecisionSupportService, ClinicalDecisionSupportService>();
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
            ?? throw new InvalidOperationException(
                "Redis connection string is required. " +
                "Configure 'ConnectionStrings:Redis' or 'Redis:ConnectionString' in appsettings.json.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "XenonClinic_";
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Adds health check services with database, API, and cache checks.
    /// </summary>
    public static IServiceCollection AddXenonHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api", tags: new[] { "api", "ready" })
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "ready" })
            .AddCheck<CacheHealthCheck>("cache", tags: new[] { "cache", "ready" });

        return services;
    }
}
