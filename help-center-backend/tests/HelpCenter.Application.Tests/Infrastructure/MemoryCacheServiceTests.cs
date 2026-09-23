using FluentAssertions;
using HelpCenter.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace HelpCenter.Application.Tests.Infrastructure;

public class MemoryCacheServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache;
    private readonly MemoryCacheService _service;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new MemoryCacheService(_memoryCache);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Set_and_TryGetValue_should_store_and_retrieve_value()
    {
        _service.Set("key1", "value1", TimeSpan.FromMinutes(5));

        var found = _service.TryGetValue<string>("key1", out var result);

        found.Should().BeTrue();
        result.Should().Be("value1");
    }

    [Fact]
    public void Remove_should_evict_item()
    {
        _service.Set("key2", "value2", TimeSpan.FromMinutes(5));
        _service.Remove("key2");

        var found = _service.TryGetValue<string>("key2", out var result);

        found.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_should_execute_factory_only_once()
    {
        int factoryCalls = 0;
        Task<string> Factory(CancellationToken ct)
        {
            factoryCalls++;
            return Task.FromResult("generated_value");
        }

        var val1 = await _service.GetOrCreateAsync("cache_key", "prefix", Factory, TimeSpan.FromMinutes(5), CancellationToken.None);
        var val2 = await _service.GetOrCreateAsync("cache_key", "prefix", Factory, TimeSpan.FromMinutes(5), CancellationToken.None);

        val1.Should().Be("generated_value");
        val2.Should().Be("generated_value");
        factoryCalls.Should().Be(1);
    }

    [Fact]
    public async Task InvalidatePrefix_should_evict_all_entries_under_prefix()
    {
        await _service.GetOrCreateAsync("key_a", "public_prefix", _ => Task.FromResult("val_a"), TimeSpan.FromMinutes(5), CancellationToken.None);
        await _service.GetOrCreateAsync("key_b", "public_prefix", _ => Task.FromResult("val_b"), TimeSpan.FromMinutes(5), CancellationToken.None);

        _service.InvalidatePrefix("public_prefix");

        _service.TryGetValue<string>("key_a", out _).Should().BeFalse();
        _service.TryGetValue<string>("key_b", out _).Should().BeFalse();
    }
}
