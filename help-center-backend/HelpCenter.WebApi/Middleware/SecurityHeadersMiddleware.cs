namespace HelpCenter.WebApi.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent XSS attacks by blocking the browser's MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevent Clickjacking attacks by blocking the page from loading in an iframe
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Prevent referrer information leakage
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Enable the browser's XSS filter
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

        // Restrict browser feature access
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        // Enforce HTTPS (active in production)
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

        // Hide server information
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}
