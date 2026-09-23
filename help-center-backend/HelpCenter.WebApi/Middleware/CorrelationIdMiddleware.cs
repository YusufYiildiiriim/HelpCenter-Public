using Serilog.Context;

namespace HelpCenter.WebApi.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        var id = ctx.Request.Headers.TryGetValue(HeaderName, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString()
            : Guid.NewGuid().ToString("N");

        ctx.Response.Headers[HeaderName] = id;
        ctx.TraceIdentifier = id;

        using (LogContext.PushProperty("CorrelationId", id))
            await _next(ctx);
    }
}
