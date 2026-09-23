using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Infrastructure.Services.Jwt;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HelpCenter.Application.Tests.Infrastructure;

public class AuthTokenServiceTests : HandlerTestBase
{
    private const string Secret = "super_secret_test_key_minimum_32_characters_long_123456";
    private readonly IConfiguration _config;
    private readonly AuthTokenService _service;

    public AuthTokenServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", Secret },
            { "JwtSettings:Issuer", "HelpCenterIssuer" },
            { "JwtSettings:Audience", "HelpCenterAudience" },
            { "JwtSettings:AccessTokenMinutes", "15" },
            { "JwtSettings:RefreshTokenDays", "7" }
        };

        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        _service = new AuthTokenService(Uow, _config);
    }

    [Fact]
    public async Task IssueTokensAsync_should_generate_tokens_and_persist_refresh_token()
    {
        var account = new Account { Email = "user@test.com", Username = "user", FirstName = "F", LastName = "L", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        var role = new Role { Name = "Admin", IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await Db.SaveChangesAsync();

        var tokens = await _service.IssueTokensAsync(user.Id, "Admin", "user@test.com", "127.0.0.1", "Chrome");

        tokens.AccessToken.Should().NotBeNullOrEmpty();
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
        tokens.AccessTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var stored = Db.RefreshTokens.FirstOrDefault(t => t.UserId == user.Id);
        stored.Should().NotBeNull();
        stored!.CreatedByIp.Should().Be("127.0.0.1");
        stored.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_should_rotate_token_and_revoke_old()
    {
        var account = new Account { Email = "rotate@test.com", Username = "rotate", FirstName = "R", LastName = "U", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        var role = new Role { Name = "Admin", IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await Db.SaveChangesAsync();

        var initial = await _service.IssueTokensAsync(user.Id, "Admin", "rotate@test.com", "127.0.0.1", "Chrome");

        var rotated = await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        rotated.AccessToken.Should().NotBeNullOrEmpty();
        rotated.RefreshToken.Should().NotBe(initial.RefreshToken);

        var oldToken = Db.RefreshTokens.First(t => t.ReplacedByTokenHash != null);
        oldToken.IsRevoked.Should().BeTrue();
        oldToken.RevokedReason.Should().Be("Rotated");
    }

    [Fact]
    public async Task RefreshAsync_on_revoked_token_should_detect_reuse_and_revoke_chain()
    {
        var account = new Account { Email = "reuse@test.com", Username = "reuse", FirstName = "R", LastName = "E", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        var role = new Role { Name = "Admin", IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await Db.SaveChangesAsync();

        var initial = await _service.IssueTokensAsync(user.Id, "Admin", "reuse@test.com", "127.0.0.1", "Chrome");
        await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        // Attempt to reuse initial revoked refresh token
        Func<Task> act = async () => await _service.RefreshAsync(initial.RefreshToken, "192.168.1.1", "Attacker");

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*yeniden kullanıldı*");

        Db.ChangeTracker.Clear();
        var activeTokens = Db.RefreshTokens.Where(t => t.UserId == user.Id && t.RevokedAt == null).ToList();
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_should_throw_when_user_has_no_role_assigned()
    {
        // Fail-closed regression test: if the user has no roles left (e.g. an admin removed all
        // their roles by sending RoleIds:[] via UpdateUser), the refresh is rejected and no Admin
        // JWT is silently produced.
        var account = new Account { Email = "norole@test.com", Username = "norole", FirstName = "N", LastName = "R", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        await Db.SaveChangesAsync();

        var initial = await _service.IssueTokensAsync(user.Id, "Admin", "norole@test.com", "127.0.0.1", "Chrome");

        Func<Task> act = async () => await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        await act.Should().ThrowAsync<UserHasNoRoleException>();
    }

    [Fact]
    public async Task RefreshAsync_should_use_the_users_actual_role_not_a_hardcoded_one()
    {
        var account = new Account { Email = "customer@test.com", Username = "customer", FirstName = "C", LastName = "U", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        var role = new Role { Name = "Customer", IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();

        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await Db.SaveChangesAsync();

        // Issued as "Admin" here on purpose — RefreshAsync must derive the role itself
        // from UserRoles, not trust/repeat whatever role the original access token had.
        var initial = await _service.IssueTokensAsync(user.Id, "Admin", "customer@test.com", "127.0.0.1", "Chrome");

        var rotated = await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(rotated.AccessToken);
        jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value.Should().Be("Customer");
    }

    [Fact]
    public async Task RevokeAsync_should_mark_token_as_revoked()
    {
        var account = new Account { Email = "logout@test.com", Username = "logout", FirstName = "L", LastName = "O", Password = "p" };
        var user = new User { Account = account, IsActive = true };
        var role = new Role { Name = "Admin", IsActive = true };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.Roles.Add(role);
        await Db.SaveChangesAsync();
        Db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await Db.SaveChangesAsync();

        var initial = await _service.IssueTokensAsync(user.Id, "Admin", "logout@test.com", "127.0.0.1", "Chrome");
        var rotated = await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        await _service.RevokeAsync(rotated.RefreshToken, "127.0.0.1");

        var tokens = Db.RefreshTokens.Where(t => t.UserId == user.Id).ToList();
        tokens.Should().OnlyContain(token => token.IsRevoked);
        tokens.Single(token => token.RevokedReason == "Logout").Should().NotBeNull();
    }

    [Fact]
    public async Task Customer_refresh_should_rotate_and_preserve_customer_claims()
    {
        var company = new Company { Name = "Customer token company" };
        var account = new Account { Email = "customer-token@test.com", Username = "customer-token", FirstName = "C", LastName = "T", Password = "p" };
        var customer = new Customer { Account = account, Company = company, IsActive = true };
        Db.Companies.Add(company);
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        var initial = await _service.IssueCustomerTokensAsync(customer.Id, company.Id, account.Email, "127.0.0.1", "Chrome");
        var rotated = await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(rotated.AccessToken);
        jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value.Should().Be("Customer");
        jwt.Claims.First(c => c.Type == "CompanyId").Value.Should().Be(company.Id.ToString());
        Db.RefreshTokens.Single(t => t.TokenHash != null && t.CustomerId == customer.Id && t.RevokedAt == null).Should().NotBeNull();
    }

    [Fact]
    public async Task Customer_refresh_reuse_should_revoke_the_customer_chain()
    {
        var company = new Company { Name = "Customer reuse company" };
        var account = new Account { Email = "customer-reuse@test.com", Username = "customer-reuse", FirstName = "C", LastName = "R", Password = "p" };
        var customer = new Customer { Account = account, Company = company, IsActive = true };
        Db.Companies.Add(company);
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        var initial = await _service.IssueCustomerTokensAsync(customer.Id, company.Id, account.Email, "127.0.0.1", "Chrome");
        await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Chrome");

        Func<Task> act = async () => await _service.RefreshAsync(initial.RefreshToken, "127.0.0.1", "Attacker");

        await act.Should().ThrowAsync<UnauthorizedException>();
        Db.ChangeTracker.Clear();
        Db.RefreshTokens.Where(t => t.CustomerId == customer.Id && t.RevokedAt == null).Should().BeEmpty();
    }
}
