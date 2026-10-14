using FluentAssertions;
using HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;
using HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Faqs;

public class FaqIdentityHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Customer_faqs_preserve_distinct_public_ids_even_with_duplicate_titles()
    {
        var first = new FAQ { Title = "Same title", Description = "First", PublicId = Guid.NewGuid(), IsActive = true };
        var second = new FAQ { Title = "Same title", Description = "Second", PublicId = Guid.NewGuid(), IsActive = true };
        Db.FAQs.AddRange(first, second);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new GetCustomerFaqsQueryHandler(Uow, Mapper);
        var result = await handler.Handle(new GetCustomerFaqsQuery(), CancellationToken.None);

        result.Select(x => x.PublicId).Should().BeEquivalentTo(new[] { first.PublicId, second.PublicId });
        result.Single(x => x.PublicId == first.PublicId).Description.Should().Be("First");
        result.Single(x => x.PublicId == second.PublicId).Description.Should().Be("Second");
    }

    [Fact]
    public async Task Public_faqs_preserve_identity_on_search_and_cached_pages_without_exposing_private_faqs()
    {
        var visible = new FAQ { Title = "Visible FAQ", PublicId = Guid.NewGuid(), IsActive = true, IsPublic = true };
        var hidden = new FAQ { Title = "Private FAQ", PublicId = Guid.NewGuid(), IsActive = true, IsPublic = false };
        Db.FAQs.AddRange(visible, hidden);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        using var memory = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, new MemoryCacheService(memory));
        var page = await handler.Handle(new GetPublicFaqsQuery(), CancellationToken.None);
        var cached = await handler.Handle(new GetPublicFaqsQuery(), CancellationToken.None);
        var searched = await handler.Handle(new GetPublicFaqsQuery(Search: "Visible"), CancellationToken.None);

        page.Items.Should().ContainSingle().Which.PublicId.Should().Be(visible.PublicId);
        cached.Items.Should().ContainSingle().Which.PublicId.Should().Be(visible.PublicId);
        searched.Items.Should().ContainSingle().Which.PublicId.Should().Be(visible.PublicId);
    }
}
