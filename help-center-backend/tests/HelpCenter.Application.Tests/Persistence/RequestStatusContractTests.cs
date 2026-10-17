using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.Data.Sqlite;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public class RequestStatusContractTests
{
    [Fact]
    public async Task Runtime_system_status_seed_keeps_the_stable_wire_contract()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new EfContext(new DbContextOptionsBuilder<EfContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await SystemDefaultsSeeder.SeedAsync(db);
        var seeds = await db.CustomerRequestStatuses.ToDictionaryAsync(x => x.Id, x => x.Name);
        var expected = new Dictionary<Guid, string>
        {
            [RequestStatusConstants.WaitingForReply] = "Cevap Bekliyor",
            [RequestStatusConstants.Replied] = "Cevaplandı",
            [RequestStatusConstants.Completed] = "Tamamlandı",
            [RequestStatusConstants.UnderTechnicalReview] = "Teknik İncelemede"
        };
        Assert.Equal(4, expected.Count);
        Assert.Equal(expected.Count, seeds.Count);
        foreach (var seed in seeds) Assert.Equal(expected[seed.Key], seed.Value);
    }
}
