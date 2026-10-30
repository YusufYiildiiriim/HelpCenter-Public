using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpCenter.Persistence.Context;

/// <summary>
/// Creates the PostgreSQL model for EF tooling without starting the API or reading local
/// environment files. A connection is resolved from an explicit --connection-string argument
/// first, then ConnectionStrings__DefaultConnection. Schema-generation commands can run without
/// either; an operation that opens a connection fails with a safe configuration message.
/// </summary>
public sealed class EfContextDesignTimeFactory : IDesignTimeDbContextFactory<EfContext>
{
    private const string ConnectionStringArgument = "--connection-string";

    public EfContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString(args);
        var options = new DbContextOptionsBuilder<EfContext>()
            .UseNpgsql(connectionString)
            .AddInterceptors(RequiredConnectionStringInterceptor.Instance)
            .Options;

        return new EfContext(options);
    }

    private static string? ResolveConnectionString(string[] args)
    {
        string? explicitConnectionString = null;
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (argument.StartsWith($"{ConnectionStringArgument}=", StringComparison.Ordinal))
            {
                if (explicitConnectionString is not null)
                    throw new InvalidOperationException($"Specify {ConnectionStringArgument} only once.");

                explicitConnectionString = argument[(ConnectionStringArgument.Length + 1)..];
                if (string.IsNullOrWhiteSpace(explicitConnectionString))
                    throw new InvalidOperationException($"Pass a PostgreSQL connection string after {ConnectionStringArgument}.");
                continue;
            }

            if (!string.Equals(argument, ConnectionStringArgument, StringComparison.Ordinal))
                continue;

            if (explicitConnectionString is not null)
                throw new InvalidOperationException($"Specify {ConnectionStringArgument} only once.");
            if (index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                throw new InvalidOperationException($"Pass a PostgreSQL connection string after {ConnectionStringArgument}.");

            explicitConnectionString = args[index + 1];
        }

        return !string.IsNullOrWhiteSpace(explicitConnectionString)
            ? explicitConnectionString
            : Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    }

    private sealed class RequiredConnectionStringInterceptor : DbConnectionInterceptor
    {
        public static readonly RequiredConnectionStringInterceptor Instance = new();

        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            EnsureConfigured(connection);
            return result;
        }

        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured(connection);
            return ValueTask.FromResult(result);
        }

        private static void EnsureConfigured(DbConnection connection)
        {
            if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                throw new InvalidOperationException(
                    "A PostgreSQL connection is required for this EF operation. Set " +
                    "ConnectionStrings__DefaultConnection or pass -- --connection-string \"<connection-string>\".");
        }
    }
}
