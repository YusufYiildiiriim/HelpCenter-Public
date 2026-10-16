using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Migrations;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class GuideModuleMigrationTests(SqlServerWebApplicationFactory factory)
{
    [Theory]
    [InlineData("exact")]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("case")]
    [InlineData("trailing")]
    public async Task Mapping_requires_exactly_one_ordinal_name_match(string scenario)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE #Modules (Id int, Name nvarchar(200)); CREATE TABLE #Guides (Module nvarchar(200), ModuleId int NULL); INSERT #Guides VALUES (N'Billing', NULL); INSERT #Modules VALUES (1, N'Billing');");
        if (scenario == "missing") await db.Database.ExecuteSqlRawAsync("DELETE #Modules;");
        if (scenario == "duplicate") await db.Database.ExecuteSqlRawAsync("INSERT #Modules VALUES (2, N'Billing');");
        if (scenario == "case") await db.Database.ExecuteSqlRawAsync("UPDATE #Modules SET Name=N'billing';");
        if (scenario == "trailing") await db.Database.ExecuteSqlRawAsync("UPDATE #Modules SET Name=N'Billing ';");
        var migration = new LinkGuidesToModules { ActiveProvider = "Microsoft.EntityFrameworkCore.SqlServer" };
        var sql = migration.UpOperations.OfType<SqlOperation>().Single().Sql.Replace("Guides", "#Guides").Replace("Modules", "#Modules");
        if (scenario == "exact")
        {
            await db.Database.ExecuteSqlRawAsync(sql);
            var id = await db.Database.SqlQueryRaw<int>("SELECT ModuleId AS Value FROM #Guides").SingleAsync();
            Assert.Equal(1, id);
            await db.Database.ExecuteSqlRawAsync("UPDATE #Modules SET Name=N'Renamed';");
            await db.Database.ExecuteSqlRawAsync(migration.DownOperations.OfType<SqlOperation>().Single().Sql.Replace("Guides", "#Guides").Replace("Modules", "#Modules"));
            Assert.Equal("Renamed", await db.Database.SqlQueryRaw<string>("SELECT Module AS Value FROM #Guides").SingleAsync());
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => db.Database.ExecuteSqlRawAsync(sql));
        await db.Database.ExecuteSqlRawAsync("DROP TABLE #Guides; DROP TABLE #Modules;");
    }
}
