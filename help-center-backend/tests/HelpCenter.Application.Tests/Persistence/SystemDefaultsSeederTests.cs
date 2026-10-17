using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class SystemDefaultsSeederTests
{
    [Fact]
    public async Task Running_twice_preserves_existing_ids_and_customized_values_without_duplicates()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new EfContext(new DbContextOptionsBuilder<EfContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();

        await SystemDefaultsSeeder.SeedAsync(db);
        var admin = await db.Roles.SingleAsync(x => x.Name == "Admin");
        var adminId = admin.Id;
        admin.Name = "Customized Admin";
        var permission = await db.RolePermissions.FirstAsync(x => x.RoleId == adminId);
        permission.IsActive = false;
        var permissionId = permission.Id;
        await db.SaveChangesAsync();

        var inserted = await SystemDefaultsSeeder.SeedAsync(db);

        Assert.Equal(0, inserted);
        Assert.Equal(4, await db.Roles.CountAsync());
        Assert.Equal(21, await db.RolePermissions.CountAsync());
        Assert.Equal(adminId, (await db.Roles.SingleAsync(x => x.Name == "Customized Admin")).Id);
        Assert.False((await db.RolePermissions.SingleAsync(x => x.Id == permissionId)).IsActive);
    }
}
