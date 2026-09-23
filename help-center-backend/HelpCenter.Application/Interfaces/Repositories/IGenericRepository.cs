using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Interfaces;

/// <summary>
/// NOTE: This interface deliberately has NO parameter like `ignoreQueryFilters` — a general flag
/// for bypassing the soft-delete filter must not be added; if a need arises, an explicitly named
/// NEW repository method should be added instead.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<T?> GetAsync(object id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool asTracking = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<IEnumerable<T>> FindWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        string[] includePaths,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        string[] includePaths,
        bool asTracking = false,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<List<TResult>> SelectAsync<TResult>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TResult>> selector,
        bool distinct = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects a single record directly to `TResult` — instead of fetching the full entity (and
    /// string-path Includes) and then mapping, only the required fields are SELECTed in SQL.
    /// `TResult` can be a DTO/record; related fields (e.g. `x.Customer!.FirstName`) are navigated
    /// directly inside the selector, and the required join is derived automatically.
    /// </summary>
    Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Same purpose as the overload above (projecting in SQL instead of fetching the full entity and
    /// mapping by hand), but reuses the existing AutoMapper profile (`IMapper.ConfigurationProvider`)
    /// instead of writing the selector by hand — via `ProjectTo&lt;TResult&gt;`. Prevents the same mapping
    /// logic from being maintained in two separate places (hand-written Select + AutoMapper profile).
    /// </summary>
    Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        IConfigurationProvider mapperConfiguration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects the first N records in descending order by a given key directly to `TResult`
    /// (for cursor/"latest N messages" style lists) — ordering and `Take` are applied at the DB
    /// level, instead of pulling all matching records into memory and cutting there.
    /// </summary>
    Task<List<TResult>> SelectTopDescendingAsync<TResult, TKey>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> orderByDescendingKey,
        int take,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default);

    Task<PaginatedResponse<T>> GetPaginatedAsync(
        Expression<Func<T, bool>>? predicate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<PaginatedResponse<TResult>> GetPaginatedProjectedAsync<TResult>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TResult>> selector,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Paginated projection using an AutoMapper profile (`ProjectTo`) — see the note above.
    /// If `orderByDescendingKey` is not given, it falls back to the same default ordering as the
    /// other `GetPaginatedAsync` methods (`Id` ascending); if given, it orders descending by that
    /// field (e.g. newest record first).
    /// </summary>
    Task<PaginatedResponse<TResult>> GetPaginatedProjectedAsync<TResult>(
        Expression<Func<T, bool>>? predicate,
        IConfigurationProvider mapperConfiguration,
        int pageNumber,
        int pageSize,
        Expression<Func<T, object>>? orderByDescendingKey = null,
        CancellationToken cancellationToken = default);

    // Note: The raw SQL methods (ExecuteScalar/ExecuteRows/ExecuteNonQuery) were deliberately
    // removed — the SQL dialect must not leak into the Application layer. For reads requiring
    // stored procedures, dedicated read repositories implemented in Persistence are used
    // (see IStatisticsReadRepository).
    Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> SoftDeleteRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    IQueryable<T> Query();
}
