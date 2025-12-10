using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Service for managing database migrations and schema operations.
/// </summary>
public interface IDatabaseMigrationService
{
    /// <summary>
    /// Checks if there are pending migrations.
    /// </summary>
    Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of pending migration names.
    /// </summary>
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of applied migration names.
    /// </summary>
    Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies all pending migrations.
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the database exists and is up to date with migrations.
    /// </summary>
    Task EnsureDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current database version/state.
    /// </summary>
    Task<DatabaseStatus> GetDatabaseStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of database migration service.
/// </summary>
public class DatabaseMigrationService : IDatabaseMigrationService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        ClinicDbContext context,
        ILogger<DatabaseMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.Any();
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.ToList();
    }

    public async Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var applied = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
        return applied.ToList();
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database migration...");

        var pendingMigrations = await GetPendingMigrationsAsync(cancellationToken);
        if (!pendingMigrations.Any())
        {
            _logger.LogInformation("No pending migrations found.");
            return;
        }

        _logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
            pendingMigrations.Count, string.Join(", ", pendingMigrations));

        try
        {
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            throw;
        }
    }

    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring database exists and is up to date...");

        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            _logger.LogInformation("Database does not exist. Creating...");
            await _context.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogInformation("Database created successfully.");
            return;
        }

        // Apply pending migrations if any
        await MigrateAsync(cancellationToken);
    }

    public async Task<DatabaseStatus> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new DatabaseStatus
        {
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            status.CanConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (status.CanConnect)
            {
                status.ProviderName = _context.Database.ProviderName;
                status.AppliedMigrations = await GetAppliedMigrationsAsync(cancellationToken);
                status.PendingMigrations = await GetPendingMigrationsAsync(cancellationToken);
                status.IsUpToDate = !status.PendingMigrations.Any();
            }
        }
        catch (Exception ex)
        {
            status.Error = ex.Message;
        }

        return status;
    }
}

/// <summary>
/// Represents the status of the database.
/// </summary>
public class DatabaseStatus
{
    /// <summary>
    /// When the status was checked.
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Whether the database can be connected to.
    /// </summary>
    public bool CanConnect { get; set; }

    /// <summary>
    /// The database provider name.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// List of applied migrations.
    /// </summary>
    public IReadOnlyList<string> AppliedMigrations { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of pending migrations.
    /// </summary>
    public IReadOnlyList<string> PendingMigrations { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the database is up to date with all migrations.
    /// </summary>
    public bool IsUpToDate { get; set; }

    /// <summary>
    /// Any error encountered during status check.
    /// </summary>
    public string? Error { get; set; }
}
