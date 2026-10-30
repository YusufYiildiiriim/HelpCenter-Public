using System.Net;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task LiveAndReadyEndpoints_ReturnOkAgainstInitializedPostgreSql()
    {
        using var client = factory.CreateClient();

        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EfContext>();
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

        Assert.Equal(
            new[] { "20261030085737_InitialPostgreSql", "20261030141555_EnforceAuditLogAppendOnly" },
            appliedMigrations);
        Assert.Equal(4, await dbContext.Roles.IgnoreQueryFilters().CountAsync());
        Assert.True(await dbContext.Accounts.IgnoreQueryFilters().AnyAsync(account => account.Email == "admin@helpcenter.com"));
        Assert.Equal(0, await SystemDefaultsSeeder.SeedAsync(dbContext));
    }
}
