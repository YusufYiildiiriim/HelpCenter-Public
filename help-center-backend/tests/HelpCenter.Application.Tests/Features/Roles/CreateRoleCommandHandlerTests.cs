using AutoMapper;
using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Roles.Commands.CreateRole;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Roles;

public class CreateRoleCommandHandlerTests : HandlerTestBase
{
    private static IMapper CreateMapper()
    {
        var mapper = Substitute.For<IMapper>();
        mapper.Map<RoleDto>(Arg.Any<object>()).Returns(call =>
        {
            var role = (Role)call.Args()[0];
            return new RoleDto { Name = role.Name };
        });
        return mapper;
    }

    [Fact]
    public async Task Should_create_role_with_default_permissions()
    {
        var mapper = CreateMapper();
        var rules = new CreateRoleRules(Uow);
        var auditLog = Substitute.For<IAuditLogWriter>();
        var handler = new CreateRoleCommandHandler(Uow, mapper, NullLogger<CreateRoleCommandHandler>.Instance, rules, auditLog);
        var command = new CreateRoleCommand { Name = "Support", Description = "Support team" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Support");
        var roles = Db.Roles.ToList();
        roles.Should().ContainSingle(r => r.Name == "Support");
    }

    [Fact]
    public async Task Should_reject_duplicate_role_name()
    {
        Db.Roles.Add(new Role { Name = "Support" });
        await Db.SaveChangesAsync();

        var mapper = CreateMapper();
        var rules = new CreateRoleRules(Uow);
        var auditLog = Substitute.For<IAuditLogWriter>();
        var handler = new CreateRoleCommandHandler(Uow, mapper, NullLogger<CreateRoleCommandHandler>.Instance, rules, auditLog);
        var command = new CreateRoleCommand { Name = "Support", Description = null };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<RoleNameAlreadyExistsException>();
    }
}
