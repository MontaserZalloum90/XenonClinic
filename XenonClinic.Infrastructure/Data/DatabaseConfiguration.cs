using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Database configuration options.
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Main connection string for the clinic database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum retry count for transient failures.
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Maximum delay between retries in seconds.
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Minimum pool size for connections.
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// Maximum pool size for connections.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Enable sensitive data logging (development only).
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Enable detailed errors (development only).
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Enable query tracking by default.
    /// Set to false for read-heavy workloads.
    /// </summary>
    public bool EnableDefaultQueryTracking { get; set; } = true;

    /// <summary>
    /// Log slow queries exceeding this threshold in milliseconds.
    /// </summary>
    public int SlowQueryThresholdMs { get; set; } = 500;
}

/// <summary>
/// Extension methods for configuring database services.
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Adds ClinicDbContext with proper configuration including retry policies and connection resilience.
    /// </summary>
    public static IServiceCollection AddClinicDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        // Get connection string with fallback
        var connectionString = configuration.GetConnectionString("ClinicDb")
            ?? databaseOptions.ConnectionString
            ?? throw new InvalidOperationException("Database connection string 'ClinicDb' not configured.");

        // Add connection pooling parameters to connection string if not present
        connectionString = EnsureConnectionPooling(connectionString, databaseOptions);

        // Register TenantContextAccessor (scoped for request lifetime)
        services.AddScoped<ITenantContextAccessor, Services.TenantContextAccessor>();

        // Register DbContext with retry policies and performance configuration
        services.AddDbContext<ClinicDbContext>((serviceProvider, options) =>
        {
            var tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable retry on transient failures
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                    errorNumbersToAdd: GetAdditionalRetryErrorNumbers());

                // Set command timeout
                sqlOptions.CommandTimeout(databaseOptions.CommandTimeoutSeconds);

                // Enable MARS for multiple result sets
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // Query tracking configuration
            if (!databaseOptions.EnableDefaultQueryTracking)
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

            // Development-only settings
            if (isDevelopment || databaseOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (isDevelopment || databaseOptions.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            // Add logging
            if (loggerFactory != null)
            {
                options.UseLoggerFactory(loggerFactory);
            }

            // Add interceptors
            var interceptorLogger = serviceProvider.GetService<ILogger<TenantIsolationSaveChangesInterceptor>>();
            if (interceptorLogger != null)
            {
                options.AddInterceptors(new TenantIsolationSaveChangesInterceptor(tenantContextAccessor, interceptorLogger));
            }
        });

        // Register database options for injection
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds DbContext for design-time migrations without tenant context.
    /// </summary>
    public static IServiceCollection AddClinicDatabaseForMigrations(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ClinicDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(120);
            });
        });

        return services;
    }

    /// <summary>
    /// Ensures connection pooling parameters are set in connection string.
    /// </summary>
    private static string EnsureConnectionPooling(string connectionString, DatabaseOptions options)
    {
        var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);

        // Set pooling parameters if not already specified
        if (!connectionString.Contains("Min Pool Size", StringComparison.OrdinalIgnoreCase))
        {
            builder.MinPoolSize = options.MinPoolSize;
        }

        if (!connectionString.Contains("Max Pool Size", StringComparison.OrdinalIgnoreCase))
        {
            builder.MaxPoolSize = options.MaxPoolSize;
        }

        if (!connectionString.Contains("Connect Timeout", StringComparison.OrdinalIgnoreCase))
        {
            builder.ConnectTimeout = 30;
        }

        if (!connectionString.Contains("Pooling", StringComparison.OrdinalIgnoreCase))
        {
            builder.Pooling = true;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Gets additional SQL Server error numbers to retry on.
    /// These include common transient errors not in default list.
    /// </summary>
    private static IEnumerable<int> GetAdditionalRetryErrorNumbers()
    {
        return new[]
        {
            // Connection errors
            -2,     // Timeout expired
            -1,     // Connection error
            233,    // Connection initialization error
            10053,  // Software caused connection abort
            10054,  // Connection reset by peer
            10060,  // Connection timed out
            40197,  // Error processing request
            40501,  // Service busy
            40613,  // Database unavailable
            49918,  // Cannot process request (elastic pool)
            49919,  // Cannot process create/update request
            49920,  // Cannot process request (subscription limit)

            // Deadlock errors
            1205,   // Transaction was deadlocked

            // Resource errors
            3960,   // Snapshot isolation conflict
            3935,   // Failed to initialize correctly
        };
    }
}

/// <summary>
/// Design-time factory for EF Core migrations.
/// </summary>
public class ClinicDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<ClinicDbContext>
{
    public ClinicDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicDbContext>();

        // Use localdb for design-time by default
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ClinicDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=XenonClinic;Trusted_Connection=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(120);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
        });

        return new ClinicDbContext(optionsBuilder.Options);
    }
}
