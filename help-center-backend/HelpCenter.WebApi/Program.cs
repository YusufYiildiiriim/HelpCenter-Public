using HelpCenter.Application.ServiceRegistration;
using HelpCenter.Infrastructure;
using HelpCenter.Persistence.ServiceRegistration;
using HelpCenter.WebApi.Configuration;
using HelpCenter.WebApi.Hubs;
using HelpCenter.WebApi.Logging;
using HelpCenter.WebApi.ServiceRegistration;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddLoggingServices(builder.Configuration);

builder.Services.AddValidatedOptions(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers(options =>
    {
        // Fills the FileUpload fields on commands from the IFormFiles in the multipart form.
        // Provider order matters: it must come before the default binders.
        options.ModelBinderProviders.Insert(0, new HelpCenter.WebApi.Binders.FileUploadModelBinderProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddWebApiServices(builder.Configuration);

builder.Services.AddHealthCheckServices(builder.Configuration);
builder.Services.AddGlobalExceptionHandling();
builder.Services.AddOpenTelemetryServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Middleware pipeline — order is critical.
app.UseMiddleware<HelpCenter.WebApi.Middleware.CorrelationIdMiddleware>();
app.UseCustomLogging();
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseHttpsRedirection();
}

app.UseCors("AllowNextJs");
app.UseMiddleware<HelpCenter.WebApi.Middleware.SecurityHeadersMiddleware>();
app.UseWebApiConfiguration();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseMiddleware<HelpCenter.WebApi.Middleware.UserLoggingMiddleware>();

app.MapHub<RequestHub>("/requestHub");
app.MapControllers();
app.MapHealthCheckEndpoints();
app.MapPrometheusScrapingEndpoint();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

Log.Information("Application started successfully.");
app.Run();

// Accessible by integration tests for WebApplicationFactory<Program>.
public partial class Program
{
    protected Program() { }
}
