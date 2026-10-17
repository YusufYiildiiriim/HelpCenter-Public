using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Auth.Queries.CustomerLogin;
using HelpCenter.Application.Features.Auth.Queries.UserLogin;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Infrastructure.Configuration;
using HelpCenter.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Auth;

public class LoginPasswordUpgradeTests : HandlerTestBase
{
    private const string Password = "Upgrade-Test-Password!";
    private readonly IAuthTokenService _tokens = Substitute.For<IAuthTokenService>();
    private readonly PasswordService _passwords = Service(2);

    private static PasswordService Service(int iterations) => new(Options.Create(new PasswordHashOptions
    {
        MemorySizeKb = 8192, Iterations = iterations, Parallelism = 1
    }));

    public LoginPasswordUpgradeTests()
    {
        var tokens = new AuthTokens("access", DateTime.UtcNow.AddMinutes(15), "refresh", DateTime.UtcNow.AddDays(7));
        _tokens.IssueTokensAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(tokens);
        _tokens.IssueCustomerTokensAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(tokens);
    }

    private Account Seed(bool customer, string hash, bool active = true, bool hasRole = true)
    {
        var account = new Account { Email = "upgrade@example.test", Username = "upgrade", Password = hash,
            IsPasswordChangeRequired = true, FirstName = "Test", LastName = "Upgrade" };
        if (customer)
            Db.Customers.Add(new Customer { Account = account, IsActive = active, Company = new Company { Name = "Upgrade Co" } });
        else
        {
            var user = new User { Account = account, IsActive = active };
            if (hasRole) user.UserRoles.Add(new UserRole { Role = new Role { Name = "Admin", IsActive = true } });
            Db.Users.Add(user);
        }
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return account;
    }

    private async Task Login(bool customer, string password)
    {
        if (customer)
            await new CustomerLoginQueryHandler(Uow, _tokens, _passwords, Mapper, NullLogger<CustomerLoginQueryHandler>.Instance)
                .Handle(new() { EmailOrUsername = "upgrade@example.test", Password = password }, default);
        else
            await new UserLoginQueryHandler(Uow, _passwords, _tokens)
                .Handle(new() { Email = "upgrade@example.test", Password = password }, default);
    }

    [Theory]
    [InlineData(false, "bcrypt")]
    [InlineData(true, "bcrypt")]
    [InlineData(false, "old-argon")]
    [InlineData(true, "old-argon")]
    [InlineData(false, "current")]
    [InlineData(true, "current")]
    public async Task Successful_login_upgrades_only_outdated_hash_and_preserves_forced_change(bool customer, string format)
    {
        var hash = format == "bcrypt" ? BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4)
            : format == "old-argon" ? Service(1).HashPassword(Password) : _passwords.HashPassword(Password);
        var account = Seed(customer, hash);
        await Login(customer, Password);
        Db.ChangeTracker.Clear();
        var saved = Db.Accounts.Find(account.Id)!;
        saved.IsPasswordChangeRequired.Should().BeTrue();
        _passwords.VerifyPassword(Password, saved.Password).Should().BeTrue();
        _passwords.NeedsRehash(saved.Password).Should().BeFalse();
        if (format == "current") saved.Password.Should().Be(hash);
        else saved.Password.Should().NotBe(hash);
        var upgraded = saved.Password;
        Db.ChangeTracker.Clear();
        await Login(customer, Password);
        Db.ChangeTracker.Clear();
        Db.Accounts.Find(account.Id)!.Password.Should().Be(upgraded);
    }

    [Theory]
    [InlineData(false, true, "wrong")]
    [InlineData(true, true, "wrong")]
    [InlineData(false, false, Password)]
    [InlineData(true, false, Password)]
    public async Task Rejected_login_never_upgrades_or_issues_tokens(bool customer, bool active, string password)
    {
        var original = BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4);
        var account = Seed(customer, original, active);
        await ((Func<Task>)(() => Login(customer, password))).Should().ThrowAsync<BaseException>();
        Db.ChangeTracker.Clear();
        Db.Accounts.Find(account.Id)!.Password.Should().Be(original);
        _tokens.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Roleless_staff_login_does_not_upgrade_password()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4);
        var account = Seed(false, hash, hasRole: false);
        await ((Func<Task>)(() => Login(false, Password))).Should().ThrowAsync<UserHasNoRoleException>();
        Db.ChangeTracker.Clear();
        Db.Accounts.Find(account.Id)!.Password.Should().Be(hash);
        _tokens.ReceivedCalls().Should().BeEmpty();
    }
}
