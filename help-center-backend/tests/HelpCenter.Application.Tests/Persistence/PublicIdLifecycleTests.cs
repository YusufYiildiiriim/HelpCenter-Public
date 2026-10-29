using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class PublicIdLifecycleTests
{
    [Fact]
    public void New_entities_use_one_stable_guid_for_id_and_public_id_before_tracking()
    {
        var first = new FAQ();
        var second = new FAQ();
        var identity = first.PublicId;

        Assert.NotEqual(Guid.Empty, identity);
        Assert.NotEqual(identity, second.PublicId);
        Assert.Equal(first.Id, identity);
        Assert.Equal(identity, first.PublicId);
        Assert.NotEqual(Guid.Empty, first.Id);
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
        var imported = new FAQ { Title = "Imported", Id = assignedId, PublicId = assignedId };
        db.FAQs.AddRange(fresh, imported);

        Assert.Equal(original, fresh.PublicId);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(original, (await db.FAQs.SingleAsync(x => x.Title == "Fresh")).PublicId);
        Assert.Equal(assignedId, (await db.FAQs.SingleAsync(x => x.Title == "Imported")).PublicId);
        Assert.DoesNotContain(await db.Roles.Select(x => x.PublicId).ToListAsync(), id => id == Guid.Empty);
    }

    [Fact]
    public void System_defaults_are_not_embedded_in_the_ef_model()
    {
        using var db = CreatePostgreSqlModel();
        var seeds = db.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .Where(type => typeof(BaseEntity).IsAssignableFrom(type.ClrType))
            .SelectMany(type => type.GetSeedData()).ToList();

        Assert.Empty(seeds);
    }

    private static EfContext CreatePostgreSqlModel() => new(new DbContextOptionsBuilder<EfContext>()
        .UseNpgsql("Host=localhost;Database=ModelOnly;Username=postgres;Password=not-used").Options);
}
