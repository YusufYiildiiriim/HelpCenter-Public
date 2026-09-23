using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Users.Commands.DeleteUser;
using HelpCenter.Application.Features.Users.Commands.UpdateUser;
using HelpCenter.Application.Features.Users.Queries.GetUsers;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Users;

public class UserHandlerTests : HandlerTestBase
{
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();

    private User SeedUser(string email, string username)
    {
        var account = new Account
        {
            Email = email,
            Username = username,
            FirstName = "OriginalFirst",
            LastName = "OriginalLast",
            Password = "hashed_pass",
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Account = account,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return user;
    }

    [Fact]
    public async Task UpdateUser_should_update_user_info_and_write_diff_log()
    {
        var user = SeedUser("user@example.com", "user.name");

        var handler = new UpdateUserCommandHandler(
            Uow, _passwordService, NullLogger<UpdateUserCommandHandler>.Instance, _auditLog, new UpdateUserRules(Uow));

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Name = "UpdatedFirst",
            LastName = "UpdatedLast",
            Email = "updated@example.com",
            IsActive = true,
            RoleIds = new List<int> { 1, 2 }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Users.Find(user.Id);
        updated!.FirstName.Should().Be("UpdatedFirst");
        updated.Email.Should().Be("updated@example.com");
        await _auditLog.Received(1).WriteDiffAsync("UserUpdated", "User", user.Id, Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateUser_should_throw_NotFoundException_when_user_not_found()
    {
        var handler = new UpdateUserCommandHandler(
            Uow, _passwordService, NullLogger<UpdateUserCommandHandler>.Instance, _auditLog, new UpdateUserRules(Uow));

        var command = new UpdateUserCommand { Id = 9999, Name = "Nobody" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task UpdateUser_should_throw_EmailAlreadyExistsException_when_email_belongs_to_another_user()
    {
        SeedUser("taken@example.com", "taken.user");
        var user = SeedUser("user@example.com", "user.name");

        var handler = new UpdateUserCommandHandler(
            Uow, _passwordService, NullLogger<UpdateUserCommandHandler>.Instance, _auditLog, new UpdateUserRules(Uow));

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Name = user.FirstName,
            LastName = user.LastName,
            Email = "taken@example.com",
            IsActive = true
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateUser_should_throw_UsernameAlreadyExistsException_when_username_belongs_to_another_user()
    {
        SeedUser("other@example.com", "taken.username");
        var user = SeedUser("user2@example.com", "user2.name");

        var handler = new UpdateUserCommandHandler(
            Uow, _passwordService, NullLogger<UpdateUserCommandHandler>.Instance, _auditLog, new UpdateUserRules(Uow));

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Name = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = "taken.username",
            IsActive = true
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UsernameAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateUser_should_succeed_when_email_and_username_unchanged_for_same_user()
    {
        var user = SeedUser("self@example.com", "self.user");

        var handler = new UpdateUserCommandHandler(
            Uow, _passwordService, NullLogger<UpdateUserCommandHandler>.Instance, _auditLog, new UpdateUserRules(Uow));

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Name = "SelfFirst",
            LastName = "SelfLast",
            Email = "self@example.com",
            Username = "self.user",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Users.Find(user.Id);
        updated!.Email.Should().Be("self@example.com");
        updated.Username.Should().Be("self.user");
    }

    [Fact]
    public async Task DeleteUser_should_soft_delete_user()
    {
        var user = SeedUser("delete_me@example.com", "del.user");

        var handler = new DeleteUserCommandHandler(
            Uow, NullLogger<DeleteUserCommandHandler>.Instance, _auditLog);

        var result = await handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        result.Should().BeTrue();
        var deleted = Db.Users.Find(user.Id);
        deleted!.IsDeleted.Should().BeTrue();
        await _auditLog.Received(1).WriteAsync("UserDeleted", "User", user.Id, Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUsers_should_return_paginated_users()
    {
        for (int i = 1; i <= 3; i++)
        {
            SeedUser($"user{i}@example.com", $"user{i}");
        }

        var handler = new GetUsersQueryHandler(Uow, Mapper);
        var result = await handler.Handle(new GetUsersQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }
}
