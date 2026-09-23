using FluentAssertions;
using HelpCenter.Application.Features.Menu.Queries.GetMenuItems;
using HelpCenter.Application.Features.Requests.Events;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Events;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Roles;

/// <summary>
/// The EF Core InMemory provider (see HandlerTestBase) generates no SQL; it runs LINQ directly over
/// .NET collections — so an expression that could never actually be TRANSLATED to SQL (e.g. a nested
/// collection projection, or computed properties like User.FirstName that proxy for Account) "works"
/// silently in InMemory but blows up in production (SQL Server). This test uses SQLite (:memory:) —
/// a provider that generates real SQL — to prove that the 4 handlers whose string-path Includes were
/// removed and converted to projections on 2026-09-06 (GetMenuItemsQueryHandler,
/// RequestCreatedEventHandler, GetRolesQueryHandler, UserPermissionsReader) really
/// can be translated.
/// </summary>
public class RolePermissionProjectionTranslationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly EfContext _db;
    private readonly UnitOfWork _uow;

    public RolePermissionProjectionTranslationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<EfContext>().UseSqlite(_connection).Options;
        _db = new EfContext(options);
        _db.Database.EnsureCreated();
        _uow = new UnitOfWork(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // The SystemDefaults seed (HasData, also applied by EnsureCreated) already defines Role Id=2 as
    // "Agent" (see RoleSeed.cs) — RequestCreatedEventHandler's hardcoded RoleId==2 lookup therefore
    // matches the real seed data without needing to create another role.
    // Since the Agent role already has several default permissions (Requests/AssignedRequests/Companies/Dashboard)
    // (see PermissionSeed.cs), tests use a NON-CONFLICTING, one-off ResourceKey.
    private const int AgentRoleId = 2;
    private const string TestResourceKey = "TestOnlyResource123";

    private async Task<int> SeedAgentUserWithReadPermissionAsync()
    {
        var account = new Account { Email = "ali@test.com", Username = "ali", FirstName = "Ali", LastName = "Agent", Password = "x", CreatedAt = DateTime.UtcNow };
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        var user = new User { AccountId = account.Id, CreatedAt = DateTime.UtcNow, IsActive = true };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var rolePermission = new RolePermission { RoleId = AgentRoleId, ResourceKey = TestResourceKey, CreatedAt = DateTime.UtcNow, IsActive = true };
        _db.RolePermissions.Add(rolePermission);
        await _db.SaveChangesAsync();

        _db.RolePermissionActions.Add(new RolePermissionAction { RolePermissionId = rolePermission.Id, Action = "Read", CreatedAt = DateTime.UtcNow, IsActive = true });
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = AgentRoleId, CreatedAt = DateTime.UtcNow, IsActive = true });
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear();
        return user.Id;
    }

    [Fact]
    public async Task GetMenuItemsQueryHandler_translates_nested_permission_projection()
    {
        var userId = await SeedAgentUserWithReadPermissionAsync();

        _db.MenuItems.Add(HelpCenter.Domain.Entities.MenuItem.Create("Test Menu", "inbox", "/test", TestResourceKey, 1));
        _db.MenuItems.Add(HelpCenter.Domain.Entities.MenuItem.Create("Yetkisiz", "lock", "/no-access", "SomeOtherResourceNoOneHas", 2));
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(userId);

        var handler = new GetMenuItemsQueryHandler(_uow, userContext);
        var items = await handler.Handle(new GetMenuItemsQuery(), CancellationToken.None);

        items.Should().Contain(i => i.ResourceKey == AppResources.AssignedRequests);
        items.Should().NotContain(i => i.ResourceKey == TestResourceKey);
        items.Should().NotContain(i => i.ResourceKey == "SomeOtherResourceNoOneHas");
    }

    [Fact]
    public async Task RequestCreatedEventHandler_translates_account_projection_and_computed_FullName()
    {
        await SeedAgentUserWithReadPermissionAsync();

        var emailDispatcher = Substitute.For<IEmailDispatcher>();
        var handler = new RequestCreatedEventHandler(_uow, emailDispatcher, NullLogger<RequestCreatedEventHandler>.Instance);

        var domainEvent = new RequestCreatedEvent(1, "Test Customer", "Test Request");
        await handler.Handle(new RequestCreatedNotification(domainEvent), CancellationToken.None);

        emailDispatcher.Received(1).EnqueueAgentNotifications(
            "Test Customer",
            "Test Request",
            Arg.Is<IEnumerable<(string Email, string FullName)>>(agents =>
                agents.Any(a => a.Email == "ali@test.com" && a.FullName == "Ali Agent")));
    }

    [Fact]
    public async Task GetRolesQueryHandler_translates_HasUsers_and_PermissionCount_aggregations()
    {
        await SeedAgentUserWithReadPermissionAsync();

        var handler = new GetRolesQueryHandler(new RoleQueryRepository(_db));
        var result = await handler.Handle(new GetRolesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        // The Agent role already has several default permissions from SystemDefaults (Requests,
        // AssignedRequests, Companies, Dashboard) — instead of an exact count, we check that "at least
        // the 1 we added was counted"; the real goal is to prove that the aggregation (Any/Count)
        // translates to SQL.
        var agentRoleDto = result.Items.Should().ContainSingle(r => r.Name == "Agent").Subject;
        agentRoleDto.HasUsers.Should().BeTrue();
        agentRoleDto.PermissionCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task UserPermissionsReader_translates_double_nested_collection_projection()
    {
        var userId = await SeedAgentUserWithReadPermissionAsync();

        var reader = new UserPermissionsReader(_uow);
        var dto = await reader.ReadAsync(userId, CancellationToken.None);

        var module = dto.Modules.Should().ContainSingle(m => m.ResourceKey == TestResourceKey).Subject;
        module.Actions.Should().ContainSingle(a => a == "Read");
    }
}
