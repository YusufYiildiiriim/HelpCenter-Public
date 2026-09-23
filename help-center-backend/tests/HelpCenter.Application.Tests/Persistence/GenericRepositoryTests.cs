using FluentAssertions;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Repositories;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public class GenericRepositoryTests : HandlerTestBase
{
    private readonly GenericRepository<Company> _repo;

    public GenericRepositoryTests()
    {
        _repo = new GenericRepository<Company>(Db);
    }

    [Fact]
    public async Task AddAsync_and_GetAsync_should_persist_and_retrieve_entity()
    {
        var company = new Company { Name = "TechCorp", Address = "Istanbul", CreatedAt = DateTime.UtcNow };

        await _repo.AddAsync(company);
        await Db.SaveChangesAsync();

        var retrieved = await _repo.GetAsync(company.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("TechCorp");
    }

    [Fact]
    public async Task FindAsync_with_predicate_should_filter_results()
    {
        Db.Companies.AddRange(
            new Company { Name = "Alpha", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Company { Name = "Beta", IsActive = false, CreatedAt = DateTime.UtcNow },
            new Company { Name = "Gamma", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await Db.SaveChangesAsync();

        var active = await _repo.FindAsync(c => c.IsActive, CancellationToken.None);

        active.Should().HaveCount(2);
        active.Select(c => c.Name).Should().Contain("Alpha", "Gamma");
    }

    [Fact]
    public async Task AnyAsync_and_CountAsync_should_evaluate_correctly()
    {
        Db.Companies.Add(new Company { Name = "Delta", CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var exists = await _repo.AnyAsync(c => c.Name == "Delta");
        var notExists = await _repo.AnyAsync(c => c.Name == "NonExistent");
        var count = await _repo.CountAsync();

        exists.Should().BeTrue();
        notExists.Should().BeFalse();
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetPaginatedAsync_should_paginate_query()
    {
        for (int i = 1; i <= 15; i++)
        {
            Db.Companies.Add(new Company { Name = $"Company_{i:D2}", CreatedAt = DateTime.UtcNow });
        }
        await Db.SaveChangesAsync();

        var page1 = await _repo.GetPaginatedAsync(null, pageNumber: 1, pageSize: 5);
        var page2 = await _repo.GetPaginatedAsync(null, pageNumber: 2, pageSize: 5);

        page1.TotalCount.Should().Be(15);
        page1.TotalPages.Should().Be(3);
        page1.Items.Should().HaveCount(5);
        page2.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPaginatedAsync_should_order_deterministically_by_id_with_no_overlap_across_pages()
    {
        // Regression test for b83c58c: paginated queries had no ORDER BY before
        // Skip/Take, so SQL Server executed "ORDER BY (SELECT 1)" — pages could
        // repeat or skip rows non-deterministically. Insert out of Id order so a
        // missing/wrong OrderBy(Id) would be caught.
        var companies = Enumerable.Range(1, 10)
            .Select(i => new Company { Name = $"Company_{i:D2}", CreatedAt = DateTime.UtcNow })
            .OrderByDescending(c => c.Name)
            .ToList();
        Db.Companies.AddRange(companies);
        await Db.SaveChangesAsync();

        var page1 = await _repo.GetPaginatedAsync(null, pageNumber: 1, pageSize: 4);
        var page2 = await _repo.GetPaginatedAsync(null, pageNumber: 2, pageSize: 4);
        var page3 = await _repo.GetPaginatedAsync(null, pageNumber: 3, pageSize: 4);

        var page1Ids = page1.Items.Select(c => c.Id).ToList();
        var page2Ids = page2.Items.Select(c => c.Id).ToList();
        var page3Ids = page3.Items.Select(c => c.Id).ToList();

        page1Ids.Should().BeInAscendingOrder();
        page2Ids.Should().BeInAscendingOrder();
        page1Ids.Should().NotIntersectWith(page2Ids);
        page2Ids.Should().NotIntersectWith(page3Ids);
        page1Ids.Concat(page2Ids).Concat(page3Ids).Should().BeInAscendingOrder()
            .And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetPaginatedAsync_with_pageSize_zero_should_return_all_items_unpaged()
    {
        for (int i = 1; i <= 12; i++)
        {
            Db.Companies.Add(new Company { Name = $"Company_{i:D2}", CreatedAt = DateTime.UtcNow });
        }
        await Db.SaveChangesAsync();

        var result = await _repo.GetPaginatedAsync(null, pageNumber: 1, pageSize: 0);

        result.Items.Should().HaveCount(12);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(1);
        result.PageSize.Should().Be(12);
    }

    [Fact]
    public async Task GetPaginatedProjectedAsync_should_paginate_and_order_deterministically()
    {
        // Insert in reverse Name order so Id ascending != Name ascending — a missing/wrong
        // OrderBy(Id) would then leak through as Name-ordered (or unordered) results.
        var companies = Enumerable.Range(1, 10)
            .Select(i => new Company { Name = $"Company_{i:D2}", CreatedAt = DateTime.UtcNow })
            .OrderByDescending(c => c.Name)
            .ToList();
        Db.Companies.AddRange(companies);
        await Db.SaveChangesAsync();

        var expectedByIdAscending = companies.OrderBy(c => c.Id).Select(c => c.Name).ToList();

        var page1 = await _repo.GetPaginatedProjectedAsync<string>(null, c => c.Name, pageNumber: 1, pageSize: 4);
        var page2 = await _repo.GetPaginatedProjectedAsync<string>(null, c => c.Name, pageNumber: 2, pageSize: 4);

        page1.TotalCount.Should().Be(10);
        page1.Items.Should().HaveCount(4);
        page2.Items.Should().HaveCount(4);
        page1.Items.Should().NotIntersectWith(page2.Items);
        page1.Items.Should().Equal(expectedByIdAscending.Take(4));
        page2.Items.Should().Equal(expectedByIdAscending.Skip(4).Take(4));
    }

    [Fact]
    public async Task GetPaginatedProjectedAsync_with_pageSize_zero_should_return_all_items_unpaged()
    {
        for (int i = 1; i <= 7; i++)
        {
            Db.Companies.Add(new Company { Name = $"Company_{i:D2}", CreatedAt = DateTime.UtcNow });
        }
        await Db.SaveChangesAsync();

        var result = await _repo.GetPaginatedProjectedAsync<string>(null, c => c.Name, pageNumber: 1, pageSize: 0);

        result.Items.Should().HaveCount(7);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_should_remove_entity()
    {
        var company = new Company { Name = "DeleteCorp", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();

        var success = await _repo.DeleteAsync(company.Id);
        await Db.SaveChangesAsync();

        success.Should().BeTrue();
        (await _repo.GetAsync(company.Id)).Should().BeNull();
    }
}
