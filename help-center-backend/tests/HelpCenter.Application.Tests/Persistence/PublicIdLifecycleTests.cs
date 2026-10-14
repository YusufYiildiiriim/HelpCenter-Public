using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class PublicIdLifecycleTests
{
    [Fact]
    public void New_entities_have_stable_distinct_public_ids_before_tracking()
    {
        var first = new FAQ();
        var second = new FAQ();
        var identity = first.PublicId;

        Assert.NotEqual(Guid.Empty, identity);
        Assert.NotEqual(identity, second.PublicId);
        Assert.Equal(identity, first.PublicId);
        Assert.Equal(0, first.Id);
    }

    [Fact]
    public async Task Tracking_saving_and_loading_preserve_new_and_existing_public_ids()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EfContext>().UseSqlite(connection).Options;
        using var db = new EfContext(options);
        await db.Database.EnsureCreatedAsync();
        var fresh = new FAQ { Title = "Fresh" };
        var original = fresh.PublicId;
        var assignedId = Guid.NewGuid();
        var imported = new FAQ { Title = "Imported", PublicId = assignedId };
        db.FAQs.AddRange(fresh, imported);

        Assert.Equal(original, fresh.PublicId);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(original, (await db.FAQs.SingleAsync(x => x.Title == "Fresh")).PublicId);
        Assert.Equal(assignedId, (await db.FAQs.SingleAsync(x => x.Title == "Imported")).PublicId);
        Assert.DoesNotContain(await db.Roles.Select(x => x.PublicId).ToListAsync(), id => id == Guid.Empty);
    }

    [Fact]
    public void System_seed_public_ids_keep_the_store_generated_sentinel()
    {
        using var db = CreateSqlServerModel();
        var seeds = db.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .Where(type => typeof(BaseEntity).IsAssignableFrom(type.ClrType))
            .SelectMany(type => type.GetSeedData()).ToList();

        Assert.NotEmpty(seeds);
        Assert.All(seeds, seed => Assert.Equal(Guid.Empty, seed[nameof(BaseEntity.PublicId)]));
    }

    [Fact]
    public void SqlServer_model_has_no_pending_migration_changes()
    {
        using var db = CreateSqlServerModel();
        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static EfContext CreateSqlServerModel() => new(new DbContextOptionsBuilder<EfContext>()
        .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true;TrustServerCertificate=true").Options);
}
