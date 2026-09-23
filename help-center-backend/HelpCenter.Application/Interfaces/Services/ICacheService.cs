namespace HelpCenter.Application.Interfaces;

/// <summary>Cache abstraction used by the application layer.</summary>
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        string prefix,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration,
        CancellationToken cancellationToken);

    void InvalidatePrefix(string prefix);

    /// <summary>Caches the value for the given duration.</summary>
    void Set<T>(string key, T value, TimeSpan absoluteExpiration);

    /// <summary>Returns the value if present in the cache.</summary>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>Removes a single key from the cache.</summary>
    void Remove(string key);
}
