using System.Collections.Concurrent;
using HelpCenter.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace HelpCenter.Infrastructure.Services.Cache;

/// <summary>
/// Implementation built on IMemoryCache that supports prefix-based bulk invalidation.
/// A single CancellationTokenSource is kept alive per prefix; all entries under that
/// prefix are bound to this source via CancellationChangeToken. When the prefix is
/// invalidated, the source is cancelled → same as the EF query filter logic: all bound
/// entries are evicted in a single atomic operation. A new source is then created for the prefix.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _prefixTokens = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        string prefix,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);

        var cts = _prefixTokens.GetOrAdd(prefix, _ => new CancellationTokenSource());
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(5)
        };
        options.AddExpirationToken(new CancellationChangeToken(cts.Token));

        _cache.Set(key, value, options);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan absoluteExpiration)
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        });
    }

    public bool TryGetValue<T>(string key, out T? value) => _cache.TryGetValue(key, out value);

    public void Remove(string key) => _cache.Remove(key);

    public void InvalidatePrefix(string prefix)
    {
        if (_prefixTokens.TryRemove(prefix, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
