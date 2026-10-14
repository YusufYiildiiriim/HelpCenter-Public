using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace HelpCenter.WebApi.Middleware;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // This scope surrounds the exception handler. Resolve identity at log time because
        // authentication and forwarded headers execute later in the request pipeline.
        using (LogContext.Push(new RequestIdentityEnricher(context)))
        {
            await _next(context);
        }
    }

    private sealed class RequestIdentityEnricher(HttpContext context) : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : $"Anonymous ({clientIp})";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", clientIp));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()));
        }
    }
}
