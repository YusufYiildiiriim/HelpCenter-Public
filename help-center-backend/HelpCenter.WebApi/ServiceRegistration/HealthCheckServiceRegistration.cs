using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HelpCenter.WebApi.ServiceRegistration;

public static class HealthCheckServiceRegistration
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "sqlserver",
                tags: new[] { "ready" })
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new() { Predicate = c => c.Tags.Contains("live") });
        app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
        return app;
    }
}
