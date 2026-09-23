using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly EfContext _efContext;

        public GenericRepository(EfContext efContext)
        {
            _efContext = efContext;
        }

        public async Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _efContext.AddAsync(entity, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            var entity = await _efContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return false;

            _efContext.Remove(entity);
            return true;
        }

        public Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var entry = _efContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                var entityType = _efContext.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyValues = primaryKey.Properties
                        .Select(p => p.PropertyInfo?.GetValue(entity))
                        .ToArray();

                    if (Array.TrueForAll(keyValues, k => k != null))
                    {
                        var existingLocal = _efContext.Set<T>().Find(keyValues);
                        if (existingLocal != null && !ReferenceEquals(existingLocal, entity))
                        {
                            _efContext.Entry(existingLocal).State = EntityState.Detached;
                        }
                    }
                }

                _efContext.Attach(entity);
            }
            entry.State = EntityState.Modified;
            return Task.FromResult(true);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _efContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            query = query.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            bool asTracking = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = asTracking ? _efContext.Set<T>() : _efContext.Set<T>().AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<IEnumerable<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            string[] includePaths,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            query = query.Where(predicate);

            foreach (var path in includePaths)
            {
                query = query.Include(path);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FirstOrDefaultWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            string[] includePaths,
            bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = asTracking ? _efContext.Set<T>() : _efContext.Set<T>().AsNoTracking();

            query = query.Where(predicate);

            foreach (var path in includePaths)
            {
                query = query.Include(path);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _efContext.Set<T>();

            return await query.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _efContext.Set<T>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            return await _efContext.Set<T>().AsNoTracking()
                .Where(predicate)
                .Select(selector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultProjectedAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            IConfigurationProvider mapperConfiguration,
            CancellationToken cancellationToken = default)
        {
            return await _efContext.Set<T>().AsNoTracking()
                .Where(predicate)
                .ProjectTo<TResult>(mapperConfiguration)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TResult>> SelectTopDescendingAsync<TResult, TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderByDescendingKey,
            int take,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            return await _efContext.Set<T>().AsNoTracking()
                .Where(predicate)
                .OrderByDescending(orderByDescendingKey)
                .Take(take)
                .Select(selector)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TResult>> SelectAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TResult>> selector,
            bool distinct = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var projected = query.Select(selector);

            if (distinct)
            {
                projected = projected.Distinct();
            }

            return await projected.ToListAsync(cancellationToken);
        }

        public async Task<PaginatedResponse<T>> GetPaginatedAsync(
            Expression<Func<T, bool>>? predicate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object>>[] includes)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 0) pageSize = 10;
            var unlimited = pageSize == 0;

            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var ordered = query.OrderBy(x => EF.Property<int>(x, "Id"));
            var items = unlimited
                ? await ordered.ToListAsync(cancellationToken)
                : await ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginatedResponse<T>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = unlimited ? (totalCount > 0 ? 1 : 0) : (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = unlimited ? 1 : pageNumber,
                PageSize = unlimited ? totalCount : pageSize
            };
        }

        public async Task<PaginatedResponse<TResult>> GetPaginatedProjectedAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TResult>> selector,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 0) pageSize = 10;
            var unlimited = pageSize == 0;

            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var projected = query.Select(selector);

            var totalCount = await projected.CountAsync(cancellationToken);

            var orderedProjected = query.OrderBy(x => EF.Property<int>(x, "Id")).Select(selector);
            var items = unlimited
                ? await orderedProjected.ToListAsync(cancellationToken)
                : await orderedProjected.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginatedResponse<TResult>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = unlimited ? (totalCount > 0 ? 1 : 0) : (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = unlimited ? 1 : pageNumber,
                PageSize = unlimited ? totalCount : pageSize
            };
        }

        public async Task<PaginatedResponse<TResult>> GetPaginatedProjectedAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            IConfigurationProvider mapperConfiguration,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderByDescendingKey = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 0) pageSize = 10;
            var unlimited = pageSize == 0;

            IQueryable<T> query = _efContext.Set<T>().AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var ordered = orderByDescendingKey != null
                ? query.OrderByDescending(orderByDescendingKey)
                : query.OrderBy(x => EF.Property<int>(x, "Id"));
            var orderedProjected = ordered.ProjectTo<TResult>(mapperConfiguration);
            var items = unlimited
                ? await orderedProjected.ToListAsync(cancellationToken)
                : await orderedProjected.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PaginatedResponse<TResult>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = unlimited ? (totalCount > 0 ? 1 : 0) : (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = unlimited ? 1 : pageNumber,
                PageSize = unlimited ? totalCount : pageSize
            };
        }

        public async Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (_efContext.Database.IsRelational())
            {
                return await _efContext.Set<T>().Where(predicate).ExecuteDeleteAsync(cancellationToken);
            }

            var entities = await _efContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);
            _efContext.Set<T>().RemoveRange(entities);
            return entities.Count;
        }

        public async Task<int> SoftDeleteRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (_efContext.Database.IsRelational())
            {
                return await _efContext.Set<T>().Where(predicate).ExecuteUpdateAsync(s => s.SetProperty(x => EF.Property<bool>(x, "IsDeleted"), true), cancellationToken);
            }

            var entities = await _efContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);
            foreach (var entity in entities)
            {
                var entry = _efContext.Entry(entity);
                if (entry.Metadata.FindProperty("IsDeleted") != null)
                {
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }
            return entities.Count;
        }

        public IQueryable<T> Query()
        {
            return _efContext.Set<T>().AsNoTracking();
        }
    }
}
