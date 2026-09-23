using FluentAssertions;
using HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Infrastructure.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Faqs;

public class PublicFaqHandlerTests : HandlerTestBase
{
    // Bkz. PublicGuideHandlerTests — MemoryCache burada da bilerek dispose edilmiyor.
    private static ICacheService Cache => new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

    private void SeedFaq(string title, int? moduleId = null, bool isPublic = true, bool isActive = true)
    {
        var faq = new FAQ
        {
            Title = title,
            Description = $"{title} açıklaması",
            ModuleId = moduleId,
            IsPublic = isPublic,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            PublicId = Guid.NewGuid()
        };
        Db.FAQs.Add(faq);
        Db.SaveChanges();
    }

    [Fact]
    public async Task Handle_should_return_only_active_public_faqs_paginated()
    {
        SeedFaq("Fatura nasıl kesilir?");
        SeedFaq("Şifre nasıl sıfırlanır?");
        SeedFaq("Gizli SSS", isPublic: false);
        SeedFaq("Pasif SSS", isActive: false);
        Db.ChangeTracker.Clear();

        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicFaqsQuery(PageNumber: 1, PageSize: 1), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(1);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_should_filter_by_moduleId()
    {
        var module = new Module { Name = "Faturalama", CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        Db.SaveChanges();

        SeedFaq("Faturalama sorusu", moduleId: module.Id);
        SeedFaq("Genel soru");
        Db.ChangeTracker.Clear();

        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicFaqsQuery(ModuleId: module.Id), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Single().Title.Should().Be("Faturalama sorusu");
    }

    [Fact]
    public async Task Handle_should_filter_by_search_term_case_insensitively()
    {
        SeedFaq("Fatura nasıl kesilir?");
        SeedFaq("Şifre nasıl sıfırlanır?");
        Db.ChangeTracker.Clear();

        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, Cache);
        var result = await handler.Handle(new GetPublicFaqsQuery(Search: "FATURA"), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Single().Title.Should().Be("Fatura nasıl kesilir?");
    }

    [Fact]
    public async Task Handle_without_search_should_be_cached_between_calls()
    {
        SeedFaq("Soru 1");
        Db.ChangeTracker.Clear();

        var cache = Cache;
        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, cache);

        var first = await handler.Handle(new GetPublicFaqsQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);
        first.TotalCount.Should().Be(1);

        SeedFaq("Soru 2");
        Db.ChangeTracker.Clear();

        var second = await handler.Handle(new GetPublicFaqsQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);
        second.TotalCount.Should().Be(1, "aramasız istek cache'lenmiş olmalı");
    }

    [Fact]
    public async Task Handle_with_search_should_bypass_cache_and_reflect_latest_data()
    {
        SeedFaq("Fatura nasıl kesilir?");
        Db.ChangeTracker.Clear();

        var cache = Cache;
        var handler = new GetPublicFaqsQueryHandler(Uow, Mapper, cache);

        var first = await handler.Handle(new GetPublicFaqsQuery(Search: "fatura"), CancellationToken.None);
        first.TotalCount.Should().Be(1);

        SeedFaq("Fatura iptali nasıl yapılır?");
        Db.ChangeTracker.Clear();

        var second = await handler.Handle(new GetPublicFaqsQuery(Search: "fatura"), CancellationToken.None);
        second.TotalCount.Should().Be(2, "arama içeren istekler her zaman DB'den taze veri okumalı");
    }
}
