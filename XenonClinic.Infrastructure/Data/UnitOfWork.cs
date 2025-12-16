using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation that coordinates database transactions
/// across multiple repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ClinicDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private IPatientRepository? _patients;
    private bool _disposed;

    public UnitOfWork(ClinicDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    /// <inheritdoc />
    public IPatientRepository Patients => _patients ??= new PatientRepository(_context);

    /// <inheritdoc />
    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        return (IRepository<T>)_repositories.GetOrAdd(type, _ => new Repository<T>(_context));
    }

    /// <inheritdoc />
    public bool HasChanges => _context.ChangeTracker.HasChanges();

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(transaction);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        foreach (var entry in _context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _repositories.Clear();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            _repositories.Clear();
            await _context.DisposeAsync();
            _disposed = true;
        }
    }
}

/// <summary>
/// Wrapper for EF Core database transaction to implement IDbTransaction.
/// </summary>
internal class DbTransactionWrapper : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    public DbTransactionWrapper(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        TransactionId = Guid.NewGuid();
    }

    /// <inheritdoc />
    public Guid TransactionId { get; }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        await _transaction.CreateSavepointAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackToSavepointAsync(name, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction.Dispose();
            }
            _disposed = true;
        }
    }
}
