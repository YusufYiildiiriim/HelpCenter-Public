using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Auth.Commands.ChangePassword;
using HelpCenter.Application.Features.Auth.Commands.ForgotPassword;
using HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;
using HelpCenter.Application.Features.Auth.Commands.UserRegister;
using HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;
using HelpCenter.Application.Features.Auth.Queries.CustomerLogin;
using HelpCenter.Application.Features.Auth.Queries.GetVerifyState;
using HelpCenter.Application.Features.Auth.Queries.UserLogin;
using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Auth;

public class AuthHandlerTests : HandlerTestBase
{
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IAuthTokenService _tokenService = Substitute.For<IAuthTokenService>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();

    private User CreateTestUser(string email, string username, string password, bool isActive = true)
    {
        var account = new Account
        {
            Email = email,
            Username = username,
            FirstName = "Test",
            LastName = "User",
            Password = password,
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Account = account,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return user;
    }

    private void AssignRole(User user, string roleName)
    {
        var role = new Role { Name = roleName, IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        Db.SaveChanges();
        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
    }

    private Customer CreateTestCustomer(string email, string username, string password, bool isActive = true)
    {
        var dummyCompany = new Company { Name = "Dummy_" + Guid.NewGuid().ToString("N")[..8], CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(dummyCompany);
        var company = new Company { Name = "AuthCo_" + Guid.NewGuid().ToString("N")[..8], CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        Db.SaveChanges();

        var account = new Account
        {
            Email = email,
            Username = username,
            FirstName = "Test",
            LastName = "Customer",
            Password = password,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            CompanyId = company.Id,
            Company = company,
            Account = account,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return customer;
    }

    [Fact]
    public async Task UserLogin_should_return_tokens_on_valid_credentials()
    {
        var user = CreateTestUser("admin@test.com", "admin", "hashed_pwd");
        AssignRole(user, "Admin");
        _passwordService.VerifyPassword("Pass123!", "hashed_pwd").Returns(true);
        _tokenService.IssueTokensAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AuthTokens("access_token", DateTime.UtcNow.AddMinutes(15), "refresh_token", DateTime.UtcNow.AddDays(7)));

        var handler = new UserLoginQueryHandler(Uow, _passwordService, _tokenService);
        var response = await handler.Handle(new UserLoginQuery { Email = "admin@test.com", Password = "Pass123!" }, CancellationToken.None);

        response.Token.Should().Be("access_token");
        response.RefreshToken.Should().Be("refresh_token");
        response.Username.Should().Be("admin");
    }

    [Fact]
    public async Task UserLogin_should_throw_UserHasNoRoleException_when_user_has_no_role_assigned()
    {
        // Fail-closed regression test: if a user has no roles at all (e.g. an admin removed all
        // their roles by sending RoleIds:[] via UpdateUser), the login is now rejected instead of
        // silently receiving an Admin JWT.
        CreateTestUser("norole@test.com", "norole", "hashed_pwd");
        _passwordService.VerifyPassword("Pass123!", "hashed_pwd").Returns(true);

        var handler = new UserLoginQueryHandler(Uow, _passwordService, _tokenService);

        Func<Task> act = async () => await handler.Handle(new UserLoginQuery { Email = "norole@test.com", Password = "Pass123!" }, CancellationToken.None);

        await act.Should().ThrowAsync<UserHasNoRoleException>();
    }

    [Fact]
    public async Task UserLogin_should_throw_UserNotFoundException_when_user_does_not_exist()
    {
        var handler = new UserLoginQueryHandler(Uow, _passwordService, _tokenService);

        Func<Task> act = async () => await handler.Handle(new UserLoginQuery { Email = "unknown@test.com", Password = "123" }, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task UserLogin_should_throw_UserInactiveException_when_user_is_inactive()
    {
        CreateTestUser("inactive@test.com", "inactive", "hashed_pwd", isActive: false);

        var handler = new UserLoginQueryHandler(Uow, _passwordService, _tokenService);

        Func<Task> act = async () => await handler.Handle(new UserLoginQuery { Email = "inactive@test.com", Password = "pwd" }, CancellationToken.None);

        await act.Should().ThrowAsync<UserInactiveException>();
    }

    [Fact]
    public async Task UserLogin_should_throw_InvalidCredentialsException_when_password_mismatch()
    {
        CreateTestUser("user@test.com", "user", "hashed_pwd");
        _passwordService.VerifyPassword("wrong", "hashed_pwd").Returns(false);

        var handler = new UserLoginQueryHandler(Uow, _passwordService, _tokenService);

        Func<Task> act = async () => await handler.Handle(new UserLoginQuery { Email = "user@test.com", Password = "wrong" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task CustomerLogin_should_return_login_response_on_success()
    {
        var customer = CreateTestCustomer("cust@test.com", "cust", "hashed_pwd");
        _passwordService.VerifyPassword("Pass123!", "hashed_pwd").Returns(true);
        _tokenService.IssueCustomerTokensAsync(
                customer.Id, customer.CompanyId, "cust@test.com", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AuthTokens("customer_jwt_token", DateTime.UtcNow.AddMinutes(15), "refresh_token", DateTime.UtcNow.AddDays(7)));

        var handler = new CustomerLoginQueryHandler(
            Uow, _tokenService, _passwordService, Mapper, NullLogger<CustomerLoginQueryHandler>.Instance);

        var result = await handler.Handle(new CustomerLoginQuery { EmailOrUsername = "cust@test.com", Password = "Pass123!" }, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().Be("customer_jwt_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.CompanyPublicId.Should().Be(customer.Company.PublicId);
    }

    [Fact]
    public async Task ChangePassword_should_update_user_password()
    {
        var user = CreateTestUser("user@test.com", "user", "old_pwd");
        _passwordService.HashPassword("NewPass123!").Returns("new_hashed_pwd");

        var handler = new ChangePasswordCommandHandler(Uow, _passwordService);
        var result = await handler.Handle(new ChangePasswordCommand { Email = "user@test.com", NewPassword = "NewPass123!", IsCustomer = false }, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Accounts.First(a => a.Email == "user@test.com");
        updated.Password.Should().Be("new_hashed_pwd");
        updated.IsPasswordChangeRequired.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePassword_should_update_customer_password()
    {
        var customer = CreateTestCustomer("cust@test.com", "cust", "old_pwd");
        _passwordService.HashPassword("NewCustomerPass123!").Returns("new_cust_pwd");

        var handler = new ChangePasswordCommandHandler(Uow, _passwordService);
        var result = await handler.Handle(new ChangePasswordCommand { Email = "cust@test.com", NewPassword = "NewCustomerPass123!", IsCustomer = true }, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Accounts.First(a => a.Email == "cust@test.com");
        updated.Password.Should().Be("new_cust_pwd");
    }

    [Fact]
    public async Task ForgotPassword_should_store_otp_and_dispatch_email()
    {
        CreateTestUser("forgot@test.com", "forgot", "pwd");

        var handler = new ForgotPasswordCommandHandler(Uow, _emailService, _cacheService);
        var result = await handler.Handle(new ForgotPasswordCommand { EmailOrUsername = "forgot@test.com", IsCustomer = false }, CancellationToken.None);

        result.Should().BeTrue();
        _cacheService.Received().Set(Arg.Is<string>(k => k.Contains("forgot@test.com")), Arg.Any<string>(), Arg.Any<TimeSpan>());
        await _emailService.Received(1).SendPasswordResetCodeAsync("forgot@test.com", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyResetCode_should_return_true_when_code_matches()
    {
        _cacheService.TryGetValue<string>("otp_reset_False_user@test.com", out Arg.Any<string?>())
            .Returns(x => { x[1] = "123456"; return true; });

        var handler = new VerifyResetCodeCommandHandler(_cacheService);
        var result = await handler.Handle(new VerifyResetCodeCommand { EmailOrUsername = "user@test.com", Code = "123456", IsCustomer = false }, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyResetCode_should_throw_when_code_is_invalid()
    {
        _cacheService.TryGetValue<string>("otp_reset_False_user@test.com", out Arg.Any<string?>())
            .Returns(x => { x[1] = "123456"; return true; });

        var handler = new VerifyResetCodeCommandHandler(_cacheService);

        Func<Task> act = async () => await handler.Handle(new VerifyResetCodeCommand { EmailOrUsername = "user@test.com", Code = "999999", IsCustomer = false }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidResetCodeException>();
    }

    [Fact]
    public async Task ResetPasswordWithCode_should_change_password_and_clear_cache()
    {
        CreateTestUser("reset@test.com", "reset", "old_pwd");
        _cacheService.TryGetValue<string>("otp_reset_False_reset@test.com", out Arg.Any<string?>())
            .Returns(x => { x[1] = "123456"; return true; });
        _passwordService.HashPassword("BrandNewPass1!").Returns("hashed_new_pass");

        var handler = new ResetPasswordWithCodeCommandHandler(Uow, _passwordService, _cacheService);
        var result = await handler.Handle(new ResetPasswordWithCodeCommand
        {
            EmailOrUsername = "reset@test.com",
            Code = "123456",
            NewPassword = "BrandNewPass1!",
            IsCustomer = false
        }, CancellationToken.None);

        result.Should().BeTrue();
        var account = Db.Accounts.First(a => a.Email == "reset@test.com");
        account.Password.Should().Be("hashed_new_pass");
        _cacheService.Received().Remove(Arg.Is<string>(k => k.Contains("reset@test.com")));
    }

    [Fact]
    public async Task UserRegister_should_register_new_user()
    {
        _passwordService.HashPassword("Pass123!").Returns("hashed_pwd");

        var handler = new UserRegisterCommandHandler(Uow, _passwordService, new UserRegisterRules(Uow), _auditLog);
        var result = await handler.Handle(new UserRegisterCommand
        {
            Email = "newuser@test.com",
            Name = "New",
            LastName = "User",
            Password = "Pass123!",
            Username = "newuser"
        }, CancellationToken.None);

        result.Should().BeTrue();
        Db.Users.Should().ContainSingle(u => u.Account.Email == "newuser@test.com");
        await _auditLog.Received(1).WriteAsync("UserRegistered", "User", Arg.Any<int>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserRegister_should_throw_ConflictException_on_duplicate_email()
    {
        CreateTestUser("existing@test.com", "existing", "pwd");

        var handler = new UserRegisterCommandHandler(Uow, _passwordService, new UserRegisterRules(Uow), _auditLog);

        Func<Task> act = async () => await handler.Handle(new UserRegisterCommand
        {
            Email = "existing@test.com",
            Name = "Another",
            LastName = "User",
            Password = "Pass123!"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetVerifyState_for_user_should_return_permissions_and_password_change_state()
    {
        var user = CreateTestUser("verify@test.com", "verify", "pwd");
        var account = Db.Accounts.Find(user.AccountId)!;
        account.IsPasswordChangeRequired = true;
        Db.Accounts.Update(account);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var reader = new UserPermissionsReader(Uow);
        var handler = new GetVerifyStateQueryHandler(Uow, reader, NullLogger<GetVerifyStateQueryHandler>.Instance);

        var result = await handler.Handle(new GetVerifyStateQuery(user.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsPasswordChangeRequired.Should().BeTrue();
        result.Permissions.Should().NotBeNull();
    }

    [Fact]
    public async Task GetVerifyState_for_customer_should_return_password_change_state_without_role_lookup()
    {
        var customer = CreateTestCustomer("verify-cust@test.com", "verifycust", "pwd");
        var account = Db.Accounts.Find(customer.AccountId)!;
        account.IsPasswordChangeRequired = true;
        Db.Accounts.Update(account);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var reader = new UserPermissionsReader(Uow);
        var handler = new GetVerifyStateQueryHandler(Uow, reader, NullLogger<GetVerifyStateQueryHandler>.Instance);

        // No role is returned anymore: a new role created from the admin panel (e.g. "Manager")
        // resolves the same way here, based purely on the User/Customer table entity,
        // without requiring a code change.
        var result = await handler.Handle(new GetVerifyStateQuery(customer.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsPasswordChangeRequired.Should().BeTrue();
    }

    [Fact]
    public async Task GetVerifyState_for_invalid_user_id_should_return_empty_response()
    {
        var reader = new UserPermissionsReader(Uow);
        var handler = new GetVerifyStateQueryHandler(Uow, reader, NullLogger<GetVerifyStateQueryHandler>.Instance);

        var result = await handler.Handle(new GetVerifyStateQuery(0), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsPasswordChangeRequired.Should().BeFalse();
        result.Permissions.Modules.Should().BeEmpty();
    }
}
