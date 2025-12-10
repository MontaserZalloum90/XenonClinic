using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Data;

/// <summary>
/// Generic repository implementation using Entity Framework Core.
/// Provides a consistent data access pattern across all entities.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ClinicDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ClinicDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        return await GetByIdInternalAsync(id, trackChanges: true, includes);
    }

    public virtual async Task<T?> GetByIdReadOnlyAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        return await GetByIdInternalAsync(id, trackChanges: false, includes);
    }

    private async Task<T?> GetByIdInternalAsync(int id, bool trackChanges, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // Use reflection to find the Id property
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equals = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        return await GetAllInternalAsync(trackChanges: true, includes);
    }

    public virtual async Task<IEnumerable<T>> GetAllReadOnlyAsync(params Expression<Func<T, object>>[] includes)
    {
        return await GetAllInternalAsync(trackChanges: false, includes);
    }

    private async Task<IEnumerable<T>> GetAllInternalAsync(bool trackChanges, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        params Expression<Func<T, object>>[] includes)
    {
        // Validate parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000; // Prevent excessive page sizes

        IQueryable<T> query = _dbSet.AsNoTracking();

        // Apply filter
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(pageNumber, pageSize);
        }

        // Apply includes
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        // Apply ordering
        if (orderBy != null)
        {
            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }
        else
        {
            // Default ordering by Id if exists
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var property = Expression.Property(parameter, "Id");
                var converted = Expression.Convert(property, typeof(object));
                var defaultOrder = Expression.Lambda<Func<T, object>>(converted, parameter);
                query = ascending
                    ? query.OrderBy(defaultOrder)
                    : query.OrderByDescending(defaultOrder);
            }
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        return await FindInternalAsync(predicate, trackChanges: true, includes);
    }

    public virtual async Task<IEnumerable<T>> FindReadOnlyAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        return await FindInternalAsync(predicate, trackChanges: false, includes);
    }

    private async Task<IEnumerable<T>> FindInternalAsync(
        Expression<Func<T, bool>> predicate,
        bool trackChanges,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
        query = query.Where(predicate);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }

        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public virtual IQueryable<T> QueryReadOnly()
    {
        return _dbSet.AsNoTracking().AsQueryable();
    }

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
