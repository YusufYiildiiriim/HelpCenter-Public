using Serilog.Context;

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
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var userName = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name
            : $"Anonymous ({clientIp})";

        using (LogContext.PushProperty("UserName", userName))
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent))
        {
            await _next(context);
        }
    }
}
