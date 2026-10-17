using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class RepositoryContractTests
{
    [Fact]
    public async Task Pagination_supports_long_primary_keys_and_soft_delete_rejects_unsupported_entities()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        using var db = new EfContext(new DbContextOptionsBuilder<EfContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        db.AuditLogs.AddRange(new AuditLog { Id = TestIds.FromInt(2), EventType = "Second" }, new AuditLog { Id = TestIds.FromInt(1), EventType = "First" });
        await db.SaveChangesAsync();
        var repo = new GenericRepository<AuditLog>(db);
        var page = await repo.GetPaginatedAsync(null, 1, 1);
        Assert.Equal(TestIds.FromInt(1), Assert.Single(page.Items).Id);
        var projected = await repo.GetPaginatedProjectedAsync(null, item => item.EventType, 2, 1);
        Assert.Equal("Second", Assert.Single(projected.Items));
        await Assert.ThrowsAsync<NotSupportedException>(() => repo.SoftDeleteRangeAsync(_ => true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Soft_delete_is_staged_and_stale_rows_fail_on_save(bool concurrentWrite)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EfContext>().UseSqlite(connection).Options;
        using var db = new EfContext(options);
        await db.Database.EnsureCreatedAsync();
        var item = new FAQ { Title = "Delete" };
        db.FAQs.Add(item);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repo = new GenericRepository<FAQ>(db);
        Assert.Equal(1, await repo.SoftDeleteRangeAsync(x => x.Id == item.Id));
        using (var observer = new EfContext(options))
        {
            Assert.False((await observer.FAQs.SingleAsync(x => x.Id == item.Id)).IsDeleted);
            // SQLite has no SQL Server rowversion. Simulate another committed writer's new token.
            if (concurrentWrite)
                await observer.Database.ExecuteSqlInterpolatedAsync($"UPDATE FAQs SET RowVersion = randomblob(8) WHERE Id = {item.Id}");
        }
        if (concurrentWrite)
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        else
        {
            await db.SaveChangesAsync();
            using var observer = new EfContext(options);
            Assert.Empty(await observer.FAQs.Where(x => x.Id == item.Id).ToListAsync());
            Assert.True((await observer.FAQs.IgnoreQueryFilters().SingleAsync(x => x.Id == item.Id)).IsDeleted);
        }
    }
}