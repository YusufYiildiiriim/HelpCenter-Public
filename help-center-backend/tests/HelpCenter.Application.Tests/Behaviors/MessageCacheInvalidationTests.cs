using HelpCenter.Application.Behaviors;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using MediatR;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Behaviors;

public class MessageCacheInvalidationTests
{
    public record Mutation : IRequest<bool>, IInvalidatesMessageCache;
    public record Read : IRequest<bool>;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Mutation_invalidates_after_success_or_partial_failure(bool fails)
    {
        var cache = Substitute.For<ICacheService>();
        var behavior = new MessageCacheInvalidationBehavior<Mutation, bool>(cache);
        RequestHandlerDelegate<bool> next = _ => fails ? Task.FromException<bool>(new InvalidOperationException()) : Task.FromResult(true);
        if (fails) await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(new(), next, default));
        else Assert.True(await behavior.Handle(new(), next, default));
        cache.Received(1).InvalidatePrefix(CacheCategory.Messages.Prefix());
    }

    [Fact]
    public async Task Reads_do_not_invalidate_messages()
    {
        var cache = Substitute.For<ICacheService>();
        await new MessageCacheInvalidationBehavior<Read, bool>(cache).Handle(new(), _ => Task.FromResult(true), default);
        cache.DidNotReceiveWithAnyArgs().InvalidatePrefix(default!);
    }
}
