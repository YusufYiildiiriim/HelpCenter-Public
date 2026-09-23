using Serilog;

namespace HelpCenter.WebApi.Logging;

public static class LoggingRegistration
{
    public static void AddLoggingServices(this ConfigureHostBuilder host, IConfiguration configuration)
    {
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"Serilog Error: {msg}"));

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .CreateLogger();

        host.UseSerilog();
    }

    public static IApplicationBuilder UseCustomLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userName = httpContext.User?.Identity?.IsAuthenticated == true
                    ? httpContext.User.Identity.Name
                    : $"Anonymous ({clientIp})";

                diagnosticContext.Set("UserName", userName);
                diagnosticContext.Set("ClientIp", clientIp);
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            };
        });

        return app;
    }
}
