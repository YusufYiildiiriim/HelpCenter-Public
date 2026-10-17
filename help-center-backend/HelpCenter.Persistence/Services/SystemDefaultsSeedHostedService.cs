using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Persistence.Services;

/// <summary>Seeds required system metadata in every environment before optional sample data.</summary>
public sealed class SystemDefaultsSeedHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SystemDefaultsSeedHostedService> _logger;

    public SystemDefaultsSeedHostedService(IServiceProvider services, ILogger<SystemDefaultsSeedHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EfContext>();
        var inserted = await SystemDefaultsSeeder.SeedAsync(context, cancellationToken);
        _logger.LogInformation("System defaults seeded; inserted {InsertedCount} missing rows.", inserted);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
