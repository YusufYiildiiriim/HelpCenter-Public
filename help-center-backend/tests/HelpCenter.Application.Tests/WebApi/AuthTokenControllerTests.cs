using FluentAssertions;
using HelpCenter.Application.Interfaces;
using HelpCenter.WebApi.Controllers.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class AuthTokenControllerTests
{
    [Fact]
    public async Task Refresh_should_rotate_the_cookie_without_exposing_the_refresh_token()
    {
        var tokens = Substitute.For<IAuthTokenService>();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        tokens.RefreshAsync("old-token", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new AuthTokens("access-token", DateTime.UtcNow.AddMinutes(15), "new-token", expiresAt));

        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "refresh_token=old-token";
        var controller = new AuthTokenController(tokens)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var result = await controller.Refresh(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        context.Response.Headers.SetCookie.ToString().Should().Contain("refresh_token=new-token");
        context.Response.Headers.SetCookie.ToString().Should().Contain("httponly");
        context.Response.Headers.SetCookie.ToString().Should().Contain("secure");
        context.Response.Headers.SetCookie.ToString().Should().Contain("samesite=lax");
        context.Response.Headers.SetCookie.ToString().Should().Contain("path=/");
        JsonSerializer.Serialize(new AuthTokens("access-token", DateTime.UtcNow, "new-token", expiresAt))
            .Should().NotContain("new-token");
        await tokens.Received(1).RefreshAsync("old-token", Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Logout_should_revoke_the_cookie_token_and_clear_it()
    {
        var tokens = Substitute.For<IAuthTokenService>();
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "refresh_token=active-token";
        var controller = new AuthTokenController(tokens)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var result = await controller.Logout(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await tokens.Received(1).RevokeAsync("active-token", Arg.Any<string?>(), Arg.Any<CancellationToken>());
        context.Response.Headers.SetCookie.ToString().Should().Contain("refresh_token=");
        context.Response.Headers.SetCookie.ToString().Should().Contain("expires=");
    }
}
