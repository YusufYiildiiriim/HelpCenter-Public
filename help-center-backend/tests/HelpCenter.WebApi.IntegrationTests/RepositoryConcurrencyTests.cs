using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Repositories;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class RepositoryConcurrencyTests(SqlServerWebApplicationFactory factory)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Soft_delete_uses_real_sql_server_rowversion(bool concurrentWrite)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        var item = new FAQ { Title = "Concurrency " + Guid.NewGuid() };
        db.FAQs.Add(item);
        await db.SaveChangesAsync();
        var before = item.RowVersion.ToArray();
        db.ChangeTracker.Clear();
        await new GenericRepository<FAQ>(db).SoftDeleteRangeAsync(x => x.Id == item.Id);
        if (concurrentWrite)
        {
            using var writerScope = factory.Services.CreateScope();
            var writer = writerScope.ServiceProvider.GetRequiredService<EfContext>();
            var changed = await writer.FAQs.SingleAsync(x => x.Id == item.Id);
            changed.Description = "Another committed writer";
            await writer.SaveChangesAsync();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => db.SaveChangesAsync());
        }
        else
        {
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var deleted = await db.FAQs.IgnoreQueryFilters().SingleAsync(x => x.Id == item.Id);
            Assert.True(deleted.IsDeleted);
            Assert.False(before.SequenceEqual(deleted.RowVersion));
        }
    }
}
