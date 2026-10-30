using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public sealed class EfContextDesignTimeFactoryTests
{
    private const string ConnectionStringEnvironmentVariable = "ConnectionStrings__DefaultConnection";

    [Fact]
    public async Task Connection_is_optional_for_model_work_and_required_for_database_operations()
    {
        var originalConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvironmentVariable, null);
            var factory = new EfContextDesignTimeFactory();
            await using var context = factory.CreateDbContext([]);

            Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
            _ = context.Model;

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.Database.OpenConnectionAsync());
            Assert.Contains("ConnectionStrings__DefaultConnection", exception.Message, StringComparison.Ordinal);
            Assert.Contains("--connection-string", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvironmentVariable, originalConnectionString);
        }
    }

    [Fact]
    public void Explicit_connection_argument_takes_precedence_over_environment_configuration()
    {
        const string environmentConnectionString = "Host=environment;Database=environment;Username=test;Password=test";
        const string explicitConnectionString = "Host=explicit;Database=explicit;Username=test;Password=test";
        var originalConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

        try
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvironmentVariable, environmentConnectionString);
            using var environmentContext = new EfContextDesignTimeFactory().CreateDbContext([]);
            Assert.Equal(environmentConnectionString, environmentContext.Database.GetDbConnection().ConnectionString);

            using var context = new EfContextDesignTimeFactory().CreateDbContext(
                ["--connection-string", explicitConnectionString]);

            Assert.Equal(explicitConnectionString, context.Database.GetDbConnection().ConnectionString);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvironmentVariable, originalConnectionString);
        }
    }
}
