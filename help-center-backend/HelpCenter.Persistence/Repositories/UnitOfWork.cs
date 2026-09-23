using System.Collections;
using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;

namespace HelpCenter.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EfContext _context;
        private Hashtable? _repositories;

        public UnitOfWork(EfContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories == null) _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance!);
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public void SetOriginalVersion<T>(T entity, byte[]? version) where T : class
        {
            if (version != null)
            {
                _context.Entry(entity).Property("RowVersion").OriginalValue = version;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
