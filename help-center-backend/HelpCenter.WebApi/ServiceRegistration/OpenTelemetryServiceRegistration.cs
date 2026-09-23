using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HelpCenter.WebApi.ServiceRegistration;

public static class OpenTelemetryServiceRegistration
{
    private const string ServiceName = "HelpCenter.WebApi";

    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(ServiceName, serviceVersion: serviceVersion))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o => o.RecordException = true)
                 .AddHttpClientInstrumentation()
                 .AddEntityFrameworkCoreInstrumentation(o =>
                 {
                     o.SetDbStatementForText = environment.IsDevelopment();
                 });

                var otlpEndpoint = configuration["Otel:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation()
                 .AddProcessInstrumentation()
                 .AddPrometheusExporter();
            });

        return services;
    }
}
