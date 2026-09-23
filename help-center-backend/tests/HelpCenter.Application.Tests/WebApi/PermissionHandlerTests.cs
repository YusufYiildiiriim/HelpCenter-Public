using System.Security.Claims;
using FluentAssertions;
using HelpCenter.Application.Interfaces;
using HelpCenter.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class PermissionHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_should_succeed_when_user_has_permission()
    {
        var dataScopeService = Substitute.For<IDataScopeService>();
        dataScopeService.HasPermissionAsync("Users", "Read").Returns(true);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IDataScopeService)).Returns(dataScopeService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var handler = new PermissionHandler(scopeFactory);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("UserId", "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        var requirement = new PermissionRequirement("Users", "Read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_should_not_succeed_when_user_lacks_permission()
    {
        var dataScopeService = Substitute.For<IDataScopeService>();
        dataScopeService.HasPermissionAsync("Users", "Delete").Returns(false);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IDataScopeService)).Returns(dataScopeService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var handler = new PermissionHandler(scopeFactory);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("UserId", "2")
        }, "TestAuth"));

        var requirement = new PermissionRequirement("Users", "Delete");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_should_not_succeed_for_unauthenticated_user()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var handler = new PermissionHandler(scopeFactory);

        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new PermissionRequirement("Users", "Read");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
