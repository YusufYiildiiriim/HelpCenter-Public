using HelpCenter.Domain.Constants;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AdminReadPermissionSeedTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task Admin_seed_grants_read_access_to_all_configured_admin_pages()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        var resources = await db.Roles
            .Where(role => role.Name == "Admin")
            .SelectMany(role => role.RolePermissions)
            .Where(permission => permission.Actions.Any(action => action.Action == PermissionActions.Read && action.IsActive && !action.IsDeleted))
            .Select(permission => permission.ResourceKey)
            .ToListAsync();

        Assert.Contains(AppResources.FAQ, resources);
        Assert.Contains(AppResources.Guide, resources);
        Assert.Contains(AppResources.Subjects, resources);
        Assert.Contains(AppResources.Statuses, resources);
        Assert.Contains(AppResources.AssignedRequests, resources);
        Assert.Contains(AppResources.Customers, resources);
        Assert.Contains(AppResources.CustomerRequests, resources);
    }
}
