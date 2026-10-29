using System.Collections.Concurrent;
using System.Net;
using System.Security.Claims;
using HelpCenter.WebApi.Logging;
using HelpCenter.WebApi.ServiceRegistration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public sealed class RequestLogEnrichmentTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handled_exception_logs_keep_late_identity_and_forwarded_ip_without_leaking_scope(bool authenticated)
    {
        var sink = new CaptureSink();
        using var logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(sink).CreateLogger();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(logger, dispose: false);
        builder.Services.AddGlobalExceptionHandling();
        await using var app = builder.Build();
        var pipeline = new ApplicationBuilder(app.Services);
        pipeline.UseCustomLogging();
        pipeline.UseExceptionHandler();
        pipeline.Use(async (context, next) =>
        {
            // Simulate the downstream authentication and forwarded-header middleware.
            if (authenticated)
                context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Yusuf")], "test"));
            context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
            await next(context);
        });
        pipeline.Run(_ => throw new InvalidOperationException("private failure detail"));
        var context = new DefaultHttpContext { RequestServices = app.Services };
        context.Connection.RemoteIpAddress = IPAddress.Loopback;
        context.Request.Headers["User-Agent"] = "log-regression-test";
        context.Request.Path = "/fault";
        context.Request.Method = "GET";
        using var response = new MemoryStream();
        context.Response.Body = response;

        await pipeline.Build()(context);

        Assert.Equal(500, context.Response.StatusCode);
        var handled = Assert.Single(sink.Events, entry => entry.MessageTemplate.Text.StartsWith("Handled exception", StringComparison.Ordinal));
        Assert.Equal(authenticated ? "Yusuf" : "Anonymous (203.0.113.7)", Scalar(handled, "UserName"));
        Assert.Equal("203.0.113.7", Scalar(handled, "ClientIp"));
        Assert.Equal("log-regression-test", Scalar(handled, "UserAgent"));
        Assert.IsType<InvalidOperationException>(handled.Exception);
        response.Position = 0;
        using var reader = new StreamReader(response, leaveOpen: true);
        Assert.DoesNotContain("private failure detail", await reader.ReadToEndAsync());

        logger.Information("AfterRequest");
        var after = Assert.Single(sink.Events, entry => entry.MessageTemplate.Text == "AfterRequest");
        Assert.False(after.Properties.ContainsKey("UserName"));
        Assert.False(after.Properties.ContainsKey("ClientIp"));
        Assert.False(after.Properties.ContainsKey("UserAgent"));
    }

    private static object? Scalar(LogEvent entry, string name) => Assert.IsType<ScalarValue>(entry.Properties[name]).Value;
    private sealed class CaptureSink : ILogEventSink
    {
        public ConcurrentQueue<LogEvent> Events { get; } = new();
        public void Emit(LogEvent logEvent) => Events.Enqueue(logEvent);
    }
}
