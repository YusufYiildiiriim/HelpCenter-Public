using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SampleData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Persistence.Services;

/// <summary>
/// Inserts sample/demo data (Acme Holding, demo customer accounts, etc.) at application startup
/// ONLY in the Development and Staging environments, idempotently. Never runs
/// in production.
///
/// This is the primary mechanism that closes the risk of sample data leaking into production: sample data
/// is no longer embedded in migrations via ModelBuilder.HasData() (migration-time OnModelCreating has no
/// access to the running application's IHostEnvironment, so no environment check could be placed there).
/// Instead, the data is inserted at runtime via SaveChangesAsync, with real IHostEnvironment
/// information — even if someone manually runs `dotnet ef database update` in production, this data is no
/// longer present in any migration, so it is never written.
/// </summary>
public class SampleDataSeedHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<SampleDataSeedHostedService> _logger;

    public SampleDataSeedHostedService(IServiceProvider serviceProvider, IHostEnvironment environment, ILogger<SampleDataSeedHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() && !_environment.IsStaging())
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EfContext>();

        try
        {
            var seeded = await SampleDataSeeder.SeedAsync(context, cancellationToken);
            if (seeded)
            {
                _logger.LogInformation("Örnek/demo veri ({Environment} ortamı) veritabanına eklendi.", _environment.EnvironmentName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Örnek/demo veri seed edilirken hata oluştu.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
