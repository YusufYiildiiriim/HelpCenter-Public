using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using HelpCenter.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

public sealed class SampleDataUtcIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("HelpCenterSampleSeedTests")
        .WithUsername("postgres")
        .WithPassword("IntegrationTests!2026")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    [Fact]
    public async Task DevelopmentSampleSeed_PersistsHistoricalUtcTimestampsAndNormalizesBusinessDates()
    {
        var options = new DbContextOptionsBuilder<EfContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        await using (var setupContext = new EfContext(options))
        {
            await setupContext.Database.MigrateAsync();
            await SystemDefaultsSeeder.SeedAsync(setupContext);
        }

        var services = new ServiceCollection()
            .AddDbContext<EfContext>(builder => builder.UseNpgsql(_postgres.GetConnectionString()))
            .BuildServiceProvider();

        try
        {
            var sampleSeedService = new SampleDataSeedHostedService(
                services,
                new TestHostEnvironment(Environments.Development),
                NullLogger<SampleDataSeedHostedService>.Instance);

            await sampleSeedService.StartAsync(CancellationToken.None);

            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<EfContext>();
            var participant = await context.ConversationParticipants
                .SingleAsync(x => x.Id == TestIds.FromInt(1));
            var company = await context.Companies.SingleAsync(x => x.Id == TestIds.FromInt(1));

            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), participant.JoinedAt);
            Assert.Equal(DateTimeKind.Utc, participant.JoinedAt.Kind);
            Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), company.CreatedAt);

            var unspecifiedExpiry = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var refreshToken = new RefreshToken
            {
                TokenHash = Guid.NewGuid().ToString("N"),
                ExpiresAt = unspecifiedExpiry
            };
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var persistedToken = await context.RefreshTokens.SingleAsync();
            Assert.Equal(DateTime.SpecifyKind(unspecifiedExpiry, DateTimeKind.Utc), persistedToken.ExpiresAt);
            Assert.Equal(DateTimeKind.Utc, persistedToken.ExpiresAt.Kind);
            Assert.Equal(3, await context.Companies.CountAsync());
        }
        finally
        {
            await services.DisposeAsync();
        }
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "HelpCenter.IntegrationTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
