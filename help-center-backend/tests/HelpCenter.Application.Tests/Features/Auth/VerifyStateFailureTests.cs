using HelpCenter.Application.Features.Auth.Queries.GetVerifyState;
using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Auth;

public sealed class VerifyStateFailureTests : HandlerTestBase
{
    [Fact]
    public async Task Permission_failure_is_logged_without_losing_password_change_state()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var failure = new InvalidOperationException("Permission store unavailable");
        uow.Repository<UserRole>().Returns(_ => throw failure);
        uow.Repository<User>().Returns(Uow.Repository<User>());
        var user = new User { Account = new Account { Email = "verify@example.test", Username = "verify", IsPasswordChangeRequired = true } };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        var logger = new RecordingLogger();
        var handler = new GetVerifyStateQueryHandler(uow, new UserPermissionsReader(uow), logger);

        var result = await handler.Handle(new GetVerifyStateQuery(user.Id), CancellationToken.None);

        Assert.Empty(result.Permissions.Modules);
        Assert.True(result.IsPasswordChangeRequired);
        AssertFailure(logger, "VerifyPermissionsReadFailed", failure, user.Id);
    }

    [Fact]
    public async Task Password_state_failure_is_logged_without_losing_permissions()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.Repository<UserRole>().Returns(Uow.Repository<UserRole>());
        var failure = new InvalidOperationException("User store unavailable");
        uow.Repository<User>().Returns(_ => throw failure);
        var role = new Role { Name = "Reader", IsActive = true };
        var permission = new RolePermission { ResourceKey = "Dashboard", Role = role };
        permission.Actions.Add(new RolePermissionAction { Action = "Read" });
        role.RolePermissions.Add(permission);
        Db.UserRoles.Add(new UserRole { UserId = HelpCenter.Application.Tests.TestIds.FromInt(42), Role = role });
        await Db.SaveChangesAsync();
        var logger = new RecordingLogger();
        var handler = new GetVerifyStateQueryHandler(uow, new UserPermissionsReader(uow), logger);

        var result = await handler.Handle(new GetVerifyStateQuery(TestIds.FromInt(42)), CancellationToken.None);

        Assert.Equal("Dashboard", Assert.Single(result.Permissions.Modules).ResourceKey);
        Assert.False(result.IsPasswordChangeRequired);
        AssertFailure(logger, "VerifyPasswordStateReadFailed", failure, TestIds.FromInt(42));
    }

    private static void AssertFailure(RecordingLogger logger, string eventName, Exception failure, Guid userId)
    {
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal(eventName, entry.Event.Name);
        Assert.Same(failure, entry.Exception);
        Assert.Contains(entry.Properties, property => property.Key == "UserId" && Equals(property.Value, userId));
    }

    private sealed class RecordingLogger : ILogger<GetVerifyStateQueryHandler>
    {
        public List<(LogLevel Level, EventId Event, Exception? Exception, KeyValuePair<string, object?>[] Properties)> Entries { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, eventId, exception, ((IEnumerable<KeyValuePair<string, object?>>)state!).ToArray()));
    }
}
