namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Coordinates the work of multiple repositories and ensures atomic operations.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the Patient repository with specialized queries.
    /// </summary>
    IPatientRepository Patients { get; }

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>A repository for the entity type.</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A disposable transaction scope.</returns>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all changes since the last SaveChanges call.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Indicates whether there are any pending changes to save.
    /// </summary>
    bool HasChanges { get; }
}

/// <summary>
/// Database transaction interface for explicit transaction management.
/// </summary>
public interface IDbTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for this transaction.
    /// </summary>
    Guid TransactionId { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a savepoint within the transaction.
    /// </summary>
    /// <param name="name">The savepoint name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back to a previously created savepoint.
    /// </summary>
    /// <param name="name">The savepoint name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default);
}
