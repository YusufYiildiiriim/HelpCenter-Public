using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SampleData;

/// <summary>
/// The SINGLE insertion point for sample/demo data (Acme Holding, demo customer accounts, etc.).
///
/// This data is no longer embedded into migrations via ModelBuilder.HasData() — HasData runs at
/// migration time (OnModelCreating) and has no access to the running application's IHostEnvironment,
/// so no environment check could be applied. That risked demo data (including the known-password
/// accounts) being injected into the prod database if "dotnet ef database update" was run manually
/// in prod.
///
/// Solution: seed data is inserted at runtime via the normal EF Core SaveChangesAsync pipeline.
/// SampleDataSeedHostedService decides when this method is called (only Development/Staging) —
/// there is NO environment check here, only idempotent insertion logic.
///
/// Note — SQL Server IDENTITY columns: the entities in these classes keep the same fixed Id
/// values as with HasData (tests and cross-references depend on that). Normal SaveChangesAsync
/// does not allow writing explicit values to an IDENTITY column (IDENTITY_INSERT OFF error), so
/// when targeting SQL Server, IDENTITY_INSERT is turned on/off around the INSERT for each table —
/// exactly what HasData's migration SQL already generated, just here at runtime instead of in a
/// migration. Other providers such as InMemory/SQLite (tests) have no such restriction and rows
/// are inserted directly.
/// </summary>
public static class SampleDataSeeder
{
    /// <summary>
    /// Inserts the sample data idempotently. If already seeded (any Company exists), it does
    /// nothing. The returned bool indicates whether seeding actually happened in this call
    /// (for logging purposes only).
    /// </summary>
    public static async Task<bool> SeedAsync(EfContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Companies.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return false;
        }

        var isSqlServer = context.Database.IsSqlServer();

        // 1. Projects & Modules (no dependencies)
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjects(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildModules(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjectModules(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildRequestSubjects(), isSqlServer, cancellationToken);

        // 2. Accounts (staff + customers) — before Company/User/Customer
        await SeedRangeAsync(context, SampleUserSeed.BuildStaffAccounts(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomerAccounts(), isSqlServer, cancellationToken);

        // 3. Companies & their licensed modules
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanies(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanyModules(), isSqlServer, cancellationToken);

        // 4. Users & customers (tied to accounts)
        await SeedRangeAsync(context, SampleUserSeed.BuildUsers(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomers(), isSqlServer, cancellationToken);

        // 5. User role/expert/project assignments
        await SeedRangeAsync(context, SampleUserSeed.BuildUserRoles(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleUserSeed.BuildModuleExperts(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleUserSeed.BuildUserProjects(), isSqlServer, cancellationToken);

        // 6. Conversations & requests
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversations(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequests(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversationParticipants(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequestMessages(), isSqlServer, cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildRequestHistories(), isSqlServer, cancellationToken);

        return true;
    }

    private static async Task SeedRangeAsync<T>(EfContext context, IReadOnlyList<T> entities, bool isSqlServer, CancellationToken cancellationToken)
        where T : class
    {
        if (entities.Count == 0)
        {
            return;
        }

        var tableName = isSqlServer
            ? context.Model.FindEntityType(typeof(T))?.GetTableName()
            : null;

        if (tableName is null)
        {
            context.Set<T>().AddRange(entities);
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        // IDENTITY_INSERT belongs to a SQL Server connection, not the DbContext. The explicit
        // transaction keeps the SET commands and SaveChanges on the same connection; the retry
        // strategy must own that transaction because SQL Server retries are enabled globally.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON", cancellationToken);

            try
            {
                context.Set<T>().AddRange(entities);
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF", cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
