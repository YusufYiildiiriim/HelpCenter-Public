namespace HelpCenter.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
    int Save();
    void SetOriginalVersion<T>(T entity, Guid version) where T : class;
}
