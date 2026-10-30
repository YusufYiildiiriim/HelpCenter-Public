using HelpCenter.Domain.Entities;
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
/// They are inserted through the normal EF Core pipeline; GUID primary keys are ordinary model
/// values and require no provider-specific identity override.
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
        var seeded = false;

        // 1. Projects & Modules (no dependencies)
        seeded |= await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjects(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleProjectModuleSeed.BuildModules(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleProjectModuleSeed.BuildProjectModules(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleProjectModuleSeed.BuildRequestSubjects(), cancellationToken);

        // 2. Accounts (staff + customers) — before Company/User/Customer
        seeded |= await SeedRangeAsync(context, SampleUserSeed.BuildStaffAccounts(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomerAccounts(), cancellationToken);

        // 3. Companies & their licensed modules
        seeded |= await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanies(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCompanyModules(), cancellationToken);

        // 4. Users & customers (tied to accounts)
        seeded |= await SeedRangeAsync(context, SampleUserSeed.BuildUsers(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleCompanyCustomerSeed.BuildCustomers(), cancellationToken);

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
        seeded |= await SeedRangeAsync(context, sampleUserRoles, cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleUserSeed.BuildModuleExperts(), cancellationToken);
        var adminUserId = await context.Users.IgnoreQueryFilters()
            .Where(user => user.Account.Email == "admin@helpcenter.com")
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);
        var sampleUserProjects = SampleUserSeed.BuildUserProjects();
        foreach (var userProject in sampleUserProjects.Where(project => project.UserId == SeedIdentity.FromLegacyInt(1)))
        {
            userProject.UserId = adminUserId;
        }
        seeded |= await SeedRangeAsync(context, sampleUserProjects, cancellationToken);

        // 6. Conversations & requests
        seeded |= await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversations(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequests(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleRequestConversationSeed.BuildConversationParticipants(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleRequestConversationSeed.BuildCustomerRequestMessages(), cancellationToken);
        seeded |= await SeedRangeAsync(context, SampleRequestConversationSeed.BuildRequestHistories(), cancellationToken);

        return seeded;
    }

    private static async Task<bool> SeedRangeAsync<T>(EfContext context, IReadOnlyList<T> entities, CancellationToken cancellationToken)
        where T : BaseEntity
    {
        if (entities.Count == 0)
        {
            return false;
        }

        var existingIds = await context.Set<T>().IgnoreQueryFilters()
            .Select(entity => entity.Id)
            .ToHashSetAsync(cancellationToken);
        var missingEntities = entities.Where(entity => !existingIds.Contains(entity.Id)).ToArray();
        if (missingEntities.Length == 0)
        {
            return false;
        }

        context.Set<T>().AddRange(missingEntities);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
