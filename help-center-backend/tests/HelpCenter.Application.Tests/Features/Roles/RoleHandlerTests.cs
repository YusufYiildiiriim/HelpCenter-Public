using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Roles.Commands.DeleteRole;
using HelpCenter.Application.Features.Roles.Commands.UpdateRole;
using HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Roles;

public class RoleHandlerTests : HandlerTestBase
{
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();

    [Fact]
    public async Task UpdateRole_should_update_role_details_and_write_diff_log()
    {
        var role = new Role { Name = "Analyst", Description = "Analyst team", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateRoleCommandHandler(Uow, new UpdateRoleRules(Uow), _auditLog);
        var command = new UpdateRoleCommand
        {
            Id = role.Id,
            Name = "Senior Analyst",
            Description = "Updated description",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Roles.Find(role.Id);
        updated!.Name.Should().Be("Senior Analyst");
        await _auditLog.Received(1).WriteDiffAsync("RoleUpdated", "Role", role.Id, Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRole_should_throw_ConflictException_on_duplicate_role_name()
    {
        var role1 = new Role { Name = "Manager", CreatedAt = DateTime.UtcNow };
        var role2 = new Role { Name = "Supervisor", CreatedAt = DateTime.UtcNow };
        Db.Roles.AddRange(role1, role2);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateRoleCommandHandler(Uow, new UpdateRoleRules(Uow), _auditLog);
        var command = new UpdateRoleCommand { Id = role2.Id, Name = "Manager" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RoleNameAlreadyExistsException>();
    }

    [Fact]
    public async Task DeleteRole_should_soft_delete_role()
    {
        var role = new Role { Name = "Temporary Role", CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteRoleCommandHandler(Uow, NullLogger<DeleteRoleCommandHandler>.Instance, _auditLog);
        var result = await handler.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        result.Should().BeTrue();
        Db.Roles.Find(role.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRole_should_throw_ConflictException_when_assigned_to_users()
    {
        var role = new Role { Name = "Assigned Role", CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        var userRole = new UserRole { Role = role, UserId = 1, CreatedAt = DateTime.UtcNow };
        Db.UserRoles.Add(userRole);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteRoleCommandHandler(Uow, NullLogger<DeleteRoleCommandHandler>.Instance, _auditLog);

        Func<Task> act = async () => await handler.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        await act.Should().ThrowAsync<RoleAssignedToUsersException>();
    }

    [Fact]
    public async Task GetRolePermissions_should_return_permission_dtos()
    {
        var role = new Role { Name = "SuperAdmin", Description = "System Admin", CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        var perm = new RolePermission { Role = role, ResourceKey = "Projects", CreatedAt = DateTime.UtcNow };
        perm.Actions.Add(new RolePermissionAction { Action = "Read", CreatedAt = DateTime.UtcNow });
        Db.RolePermissions.Add(perm);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new GetRolePermissionsQueryHandler(Uow);
        var result = await handler.Handle(new GetRolePermissionsQuery(role.Id), CancellationToken.None);

        result.Should().NotBeEmpty();
        result.First().ResourceKey.Should().Be("Projects");
        result.First().Actions.Should().Contain("Read");
    }

    [Fact]
    public async Task GetRoles_should_return_active_roles()
    {
        Db.Roles.AddRange(
            new Role { Name = "RoleA", CreatedAt = DateTime.UtcNow },
            new Role { Name = "RoleB", CreatedAt = DateTime.UtcNow }
        );
        await Db.SaveChangesAsync();

        var handler = new GetRolesQueryHandler(new RoleQueryRepository(Db));
        var result = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }
}
