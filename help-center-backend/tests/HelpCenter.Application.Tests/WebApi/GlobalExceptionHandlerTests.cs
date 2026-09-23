using System.IO;
using System.Text.Json;
using FluentAssertions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Exceptions;
using HelpCenter.WebApi.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class GlobalExceptionHandlerTests
{
    private readonly IHostEnvironment _devEnv = Substitute.For<IHostEnvironment>();
    private readonly IHostEnvironment _prodEnv = Substitute.For<IHostEnvironment>();

    public GlobalExceptionHandlerTests()
    {
        _devEnv.EnvironmentName.Returns("Development");
        _prodEnv.EnvironmentName.Returns("Production");
    }

    private async Task<(int statusCode, ApiResponse payload)> ExecuteHandlerAsync(Exception exception, bool isDev = true)
    {
        var env = isDev ? _devEnv : _prodEnv;
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env);

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();

        responseStream.Seek(0, SeekOrigin.Begin);
        var payload = await JsonSerializer.DeserializeAsync<ApiResponse>(responseStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return (context.Response.StatusCode, payload!);
    }

    [Fact]
    public async Task TryHandleAsync_should_map_NotFoundException_to_404()
    {
        var (status, payload) = await ExecuteHandlerAsync(new NotFoundException("Item missing"));

        status.Should().Be(404);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_BadRequestException_to_400()
    {
        var (status, payload) = await ExecuteHandlerAsync(new BadRequestException("Invalid data"));

        status.Should().Be(400);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("BadRequest");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_UnauthorizedException_to_401()
    {
        var (status, payload) = await ExecuteHandlerAsync(new UnauthorizedException("Token expired"));

        status.Should().Be(401);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_ForbiddenException_to_403()
    {
        var (status, payload) = await ExecuteHandlerAsync(new ForbiddenException("Access denied"));

        status.Should().Be(403);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("Forbidden");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_ConflictException_to_409()
    {
        var (status, payload) = await ExecuteHandlerAsync(new ConflictException("Already exists"));

        status.Should().Be(409);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("Conflict");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_ValidationException_to_400_with_details()
    {
        var errors = new List<string> { "Field is required", "Field must be positive" };
        var (status, payload) = await ExecuteHandlerAsync(new ValidationException("Validation error", errors));

        status.Should().Be(400);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("ValidationError");
        payload.Error.Details.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public async Task TryHandleAsync_should_map_DbUpdateConcurrencyException_to_409()
    {
        var (status, payload) = await ExecuteHandlerAsync(new DbUpdateConcurrencyException("Concurrency clash"));

        status.Should().Be(409);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("ConcurrencyConflict");
    }

    [Fact]
    public async Task TryHandleAsync_should_hide_internal_message_in_production()
    {
        var (status, payload) = await ExecuteHandlerAsync(new InvalidOperationException("Sensitive DB error"), isDev: false);

        status.Should().Be(500);
        payload.Success.Should().BeFalse();
        payload.Message.Should().Be("Sunucu hatası. Lütfen daha sonra tekrar deneyin.");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_specialized_UserNotFoundException()
    {
        var (status, payload) = await ExecuteHandlerAsync(new UserNotFoundException(), isDev: false);

        status.Should().Be(404);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("UserNotFound");
        payload.Message.Should().Be("Kullanıcı hesabı bulunamadı.");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_specialized_CustomerNotFoundException()
    {
        var (status, payload) = await ExecuteHandlerAsync(new CustomerNotFoundException(), isDev: false);

        status.Should().Be(404);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("CustomerNotFound");
        payload.Message.Should().Be("Müşteri hesabı bulunamadı.");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_specialized_UserInactiveException()
    {
        var (status, payload) = await ExecuteHandlerAsync(new UserInactiveException(), isDev: false);

        status.Should().Be(400);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("UserInactive");
        payload.Message.Should().Be("Kullanıcı hesabınız aktif değil.");
    }

    [Fact]
    public async Task TryHandleAsync_should_map_specialized_InvalidCredentialsException()
    {
        var (status, payload) = await ExecuteHandlerAsync(new InvalidCredentialsException(), isDev: false);

        status.Should().Be(401);
        payload.Success.Should().BeFalse();
        payload.Error!.Code.Should().Be("InvalidCredentials");
        payload.Message.Should().Be("Hatalı kullanıcı adı/e-posta veya şifre.");
    }
}
