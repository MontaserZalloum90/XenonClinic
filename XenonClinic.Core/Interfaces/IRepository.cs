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
    /// Gets an entity by its ID without change tracking.
    /// Use for read-only scenarios.
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<T?> GetByIdReadOnlyAsync(int id, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets all entities without change tracking.
    /// Use for read-only scenarios with large datasets.
    /// </summary>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> GetAllReadOnlyAsync(params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="predicate">Optional filter predicate</param>
    /// <param name="orderBy">Optional ordering expression</param>
    /// <param name="ascending">Sort direction (default ascending)</param>
    /// <param name="includes">Optional include expressions</param>
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Finds entities matching a predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Finds entities matching a predicate without change tracking.
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="includes">Optional include expressions for eager loading</param>
    Task<IEnumerable<T>> FindReadOnlyAsync(
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
    /// Consider using IUnitOfWork for cross-repository transactions.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Gets a queryable for advanced queries without change tracking.
    /// Use sparingly - prefer the other methods for most operations.
    /// </summary>
    IQueryable<T> QueryReadOnly();

    /// <summary>
    /// Gets a queryable for advanced queries with change tracking.
    /// Use sparingly - prefer the other methods for most operations.
    /// </summary>
    IQueryable<T> Query();
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates an empty paged result.
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber, int pageSize) => new()
    {
        Items = Array.Empty<T>(),
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalCount = 0
    };
}
