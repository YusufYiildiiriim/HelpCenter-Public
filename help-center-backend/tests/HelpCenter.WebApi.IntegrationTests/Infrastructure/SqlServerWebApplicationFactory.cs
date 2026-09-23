using HelpCenter.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests.Infrastructure;

public sealed class SqlServerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SqlServerPassword = "IntegrationTests!2026";

    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithDatabase("HelpCenterIntegrationTests")
        .WithPassword(SqlServerPassword)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _sqlServer.GetConnectionString(),
                ["JwtSettings:Issuer"] = "HelpCenter.IntegrationTests",
                ["JwtSettings:Audience"] = "HelpCenter.IntegrationTests",
                ["JwtSettings:SecretKey"] = "IntegrationTestsOnlySecretKeyThatIsAtLeastThirtyTwoCharacters",
                ["SmtpSettings:Host"] = "localhost",
                ["SmtpSettings:Port"] = "2525",
                ["SmtpSettings:Username"] = "integration-tests",
                ["SmtpSettings:Password"] = "integration-tests",
                ["SmtpSettings:FromAddress"] = "integration-tests@example.test",
                ["AppSettings:FrontendUrl"] = "http://localhost:3000"
            };

            configuration.AddInMemoryCollection(settings);
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        // The top-level Program validates these settings while it is being constructed, before
        // WebApplicationFactory's ConfigureAppConfiguration callback is applied. Supplying them
        // through this test process's environment makes the real startup path deterministic.
        SetProcessConfiguration("ConnectionStrings__DefaultConnection", _sqlServer.GetConnectionString());
        SetProcessConfiguration("JwtSettings__Issuer", "HelpCenter.IntegrationTests");
        SetProcessConfiguration("JwtSettings__Audience", "HelpCenter.IntegrationTests");
        SetProcessConfiguration("JwtSettings__SecretKey", "IntegrationTestsOnlySecretKeyThatIsAtLeastThirtyTwoCharacters");
        SetProcessConfiguration("SmtpSettings__Host", "localhost");
        SetProcessConfiguration("SmtpSettings__Port", "2525");
        SetProcessConfiguration("SmtpSettings__Username", "integration-tests");
        SetProcessConfiguration("SmtpSettings__Password", "integration-tests");
        SetProcessConfiguration("SmtpSettings__FromAddress", "integration-tests@example.test");
        SetProcessConfiguration("AppSettings__FrontendUrl", "http://localhost:3000");

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EfContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static void SetProcessConfiguration(string key, string value)
        => Environment.SetEnvironmentVariable(key, value);

    public new async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
        Dispose();
    }
}
