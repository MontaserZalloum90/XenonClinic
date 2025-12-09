using System.Linq.Expressions;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access operations.
/// Provides a consistent abstraction over Entity Framework DbContext.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets the first entity matching a predicate, or null.
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts entities matching an optional predicate.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Removes multiple entities.
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Gets a queryable for advanced queries.
    /// Use sparingly - prefer the other methods for most operations.
    /// </summary>
    IQueryable<T> Query();
}
