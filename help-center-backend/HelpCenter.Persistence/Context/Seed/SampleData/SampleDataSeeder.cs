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
/// Sample entities carry fixed GUID identities so their cross-references remain deterministic.
/// They are inserted through the normal EF Core pipeline; GUID primary keys are not SQL Server
/// IDENTITY columns and therefore must not use IDENTITY_INSERT.
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

        // 1. Projects & Modules (no dependencies)
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjects(), cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildModules(), cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjectModules(), cancellationToken);
        await SeedRangeAsync(context, SampleProjectModuleSeed.BuildRequestSubjects(), cancellationToken);

        // 2. Accounts (staff + customers) — before Company/User/Customer
        await SeedRangeAsync(context, SampleUserSeed.BuildStaffAccounts(), cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomerAccounts(), cancellationToken);

        // 3. Companies & their licensed modules
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanies(), cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanyModules(), cancellationToken);

        // 4. Users & customers (tied to accounts)
        await SeedRangeAsync(context, SampleUserSeed.BuildUsers(), cancellationToken);
        await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomers(), cancellationToken);

        // 5. User role/expert/project assignments
        var rolesByName = await context.Roles.IgnoreQueryFilters()
            .ToDictionaryAsync(role => role.Name, role => role.Id, StringComparer.Ordinal, cancellationToken);
        var sampleUserRoles = SampleUserSeed.BuildUserRoles();
        foreach (var userRole in sampleUserRoles)
        {
            userRole.RoleId = userRole.RoleId switch
            {
                var id when id == SeedIdentity.FromLegacyInt(2) => rolesByName["Agent"],
                var id when id == SeedIdentity.FromLegacyInt(4) => rolesByName["Expert"],
                _ => userRole.RoleId
            };
        }
        await SeedRangeAsync(context, sampleUserRoles, cancellationToken);
        await SeedRangeAsync(context, SampleUserSeed.BuildModuleExperts(), cancellationToken);
        var adminUserId = await context.Users.IgnoreQueryFilters()
            .Where(user => user.Account.Email == "admin@helpcenter.com")
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);
        var sampleUserProjects = SampleUserSeed.BuildUserProjects();
        foreach (var userProject in sampleUserProjects.Where(project => project.UserId == SeedIdentity.FromLegacyInt(1)))
        {
            userProject.UserId = adminUserId;
        }
        await SeedRangeAsync(context, sampleUserProjects, cancellationToken);

        // 6. Conversations & requests
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversations(), cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequests(), cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversationParticipants(), cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequestMessages(), cancellationToken);
        await SeedRangeAsync(context, SampleRequestConversationSeed.BuildRequestHistories(), cancellationToken);

        return true;
    }

    private static async Task SeedRangeAsync<T>(EfContext context, IReadOnlyList<T> entities, CancellationToken cancellationToken)
        where T : class
    {
        if (entities.Count == 0)
        {
            return;
        }

        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }
}
