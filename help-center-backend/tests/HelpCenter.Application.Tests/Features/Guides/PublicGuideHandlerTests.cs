using FluentAssertions;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Guides;

/// <summary>
/// The public portal home page (help-center-ui/src/app/page.tsx) now fetches the guide list
/// paginated. The highest-risk point is the PreviousGuidePublicId chain, which must always resolve
/// correctly even if it extends beyond the page boundary (GuideReaderModal forward/back navigation) —
/// these tests primarily verify that.
/// </summary>
public class PublicGuideHandlerTests : HandlerTestBase
{
    // The MemoryCache is intentionally not disposed here: HandlerTestBase.Dispose() is not virtual,
    // and shadowing it would bypass the cleanup xUnit invokes through IDisposable. Lifetimes are
    // short within test scope; the GC is sufficient.
    private static ICacheService Cache => new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

    private Guide SeedGuide(string title, string module, bool isPublic = true, bool isActive = true, int? previousGuideId = null)
    {
        var guide = new Guide
        {
            Title = title,
            Description = $"{title} açıklaması",
            Module = module,
            IsPublic = isPublic,
            IsActive = isActive,
            PreviousGuideId = previousGuideId,
            CreatedAt = DateTime.UtcNow,
            PublicId = Guid.NewGuid()
        };
        Db.Guides.Add(guide);
        Db.SaveChanges();
        return guide;
    }

    [Fact]
    public async Task Handle_should_return_only_active_public_guides_paginated()
    {
        SeedGuide("Adım 1", "Modül A");
        SeedGuide("Adım 2", "Modül A");
        SeedGuide("Gizli Adım", "Modül A", isPublic: false);
        SeedGuide("Pasif Adım", "Modül A", isActive: false);
        Db.ChangeTracker.Clear();

        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicGuidesQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(g => g.Title == "Adım 1" || g.Title == "Adım 2");
    }

    [Fact]
    public async Task Handle_should_resolve_previous_guide_chain_across_page_boundary()
    {
        var a = SeedGuide("A - Kurulum", "Modül A");
        var b = SeedGuide("B - Yapılandırma", "Modül A", previousGuideId: a.Id);
        var c = SeedGuide("C - Test", "Modül A", previousGuideId: b.Id);
        Db.ChangeTracker.Clear();

        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, Cache);

        // Page 1: A and B are on the same page -> B's previous step should resolve to A.
        var page1 = await handler.Handle(new GetPublicGuidesQuery(PageNumber: 1, PageSize: 2), CancellationToken.None);
        page1.Items.Should().HaveCount(2);
        var bDto = page1.Items.Single(g => g.Title == "B - Yapılandırma");
        bDto.PreviousGuidePublicId.Should().Be(a.PublicId);

        // Page 2: C is alone; its previous step (B) is NOT on this page — it should still resolve correctly.
        var page2 = await handler.Handle(new GetPublicGuidesQuery(PageNumber: 2, PageSize: 2), CancellationToken.None);
        page2.Items.Should().HaveCount(1);
        var cDto = page2.Items.Single();
        cDto.Title.Should().Be("C - Test");
        cDto.PreviousGuidePublicId.Should().Be(b.PublicId);
    }

    [Fact]
    public async Task Handle_with_pageSize_zero_should_return_all_matching_guides_unlimited()
    {
        SeedGuide("A", "Modül A");
        SeedGuide("B", "Modül A");
        SeedGuide("C", "Modül B");
        Db.ChangeTracker.Clear();

        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicGuidesQuery(Module: "Modül A", PageSize: 0), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(g => g.Module == "Modül A");
    }

    [Fact]
    public async Task Handle_should_filter_by_search_term_case_insensitively()
    {
        SeedGuide("Fatura Oluşturma", "Modül A");
        SeedGuide("Kullanıcı Ekleme", "Modül A");
        Db.ChangeTracker.Clear();

        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicGuidesQuery(Search: "fatura"), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Single().Title.Should().Be("Fatura Oluşturma");
    }

    [Fact]
    public async Task Handle_without_search_should_be_cached_between_calls()
    {
        SeedGuide("A", "Modül A");
        Db.ChangeTracker.Clear();

        var cache = Cache;
        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, cache);

        var first = await handler.Handle(new GetPublicGuidesQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);
        first.TotalCount.Should().Be(1);

        // Add a new guide to the DB; the cached result must not change.
        SeedGuide("B", "Modül A");
        Db.ChangeTracker.Clear();

        var second = await handler.Handle(new GetPublicGuidesQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);
        second.TotalCount.Should().Be(1, "requests without a search term should be cached");
    }

    [Fact]
    public async Task Handle_with_search_should_bypass_cache_and_reflect_latest_data()
    {
        SeedGuide("Fatura Oluşturma", "Modül A");
        Db.ChangeTracker.Clear();

        var cache = Cache;
        var handler = new GetPublicGuidesQueryHandler(Uow, Mapper, cache);

        var first = await handler.Handle(new GetPublicGuidesQuery(Search: "fatura"), CancellationToken.None);
        first.TotalCount.Should().Be(1);

        SeedGuide("Fatura İptali", "Modül A");
        Db.ChangeTracker.Clear();

        var second = await handler.Handle(new GetPublicGuidesQuery(Search: "fatura"), CancellationToken.None);
        second.TotalCount.Should().Be(2, "requests with a search term should always read fresh data from the DB");
    }
}
