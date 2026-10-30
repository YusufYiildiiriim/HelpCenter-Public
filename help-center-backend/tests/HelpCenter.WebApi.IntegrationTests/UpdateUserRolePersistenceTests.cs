using HelpCenter.Application.Features.Users.Commands.UpdateUser;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class UpdateUserRolePersistenceTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task UpdateUser_persists_replaced_roles()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        var (userId, rowVersion) = await CreateUserWithAgentRoleAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(CreateCommand(userId, rowVersion, TestIds.FromInt(4)));
        }

        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<EfContext>();
        var roles = await db.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();
        Assert.Equal([TestIds.FromInt(4)], roles);
    }

    [Fact]
    public async Task UpdateUser_with_stale_row_version_keeps_existing_roles()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        var (userId, staleRowVersion) = await CreateUserWithAgentRoleAsync();

        using (var concurrentScope = factory.Services.CreateScope())
        {
            var db = concurrentScope.ServiceProvider.GetRequiredService<EfContext>();
            var user = await db.Users.Include(x => x.Account).SingleAsync(x => x.Id == userId);
            user.IsActive = false;
            user.Account.FirstName = "Concurrent update";
            await db.SaveChangesAsync();
        }

        using (var staleScope = factory.Services.CreateScope())
        {
            var mediator = staleScope.ServiceProvider.GetRequiredService<IMediator>();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                mediator.Send(CreateCommand(userId, staleRowVersion, TestIds.FromInt(4))));
        }

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<EfContext>();
        var roles = await verifyDb.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();
        Assert.Equal([TestIds.FromInt(2)], roles);
        Assert.False(await verifyDb.Users.Where(x => x.Id == userId).Select(x => x.IsActive).SingleAsync());
        Assert.Equal("Concurrent update", await verifyDb.Users.Where(x => x.Id == userId).Select(x => x.Account.FirstName).SingleAsync());
    }

    private async Task<(Guid UserId, Guid RowVersion)> CreateUserWithAgentRoleAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var user = User.Register($"role-update-{suffix}@example.test", "Integration", "Roles", "not-used", $"role-update-{suffix}");
        db.Users.Add(user);
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = TestIds.FromInt(2) });
        await db.SaveChangesAsync();
        return (user.Id, user.RowVersion);
    }

    private static UpdateUserCommand CreateCommand(Guid userId, Guid rowVersion, Guid roleId) => new()
    {
        Id = userId,
        Name = "Updated",
        LastName = "User",
        Email = $"updated-{userId:N}@example.test",
        IsActive = true,
        RowVersion = rowVersion,
        RoleIds = [roleId]
    };
}
