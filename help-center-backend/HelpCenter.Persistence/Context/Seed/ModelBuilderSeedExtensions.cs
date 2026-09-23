using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed;

public static class ModelBuilderSeedExtensions
{
    /// <summary>
    /// Seeds the metadata required for the system to function (Roles, Permissions, Statuses, Menus, Organization, Admin).
    /// Runs at migration time (HasData), so it is applied in EVERY environment (including prod) — this is
    /// intentional: these are not demo data, they are metadata required for the system to work.
    ///
    /// Sample/demo data (Acme Holding, etc.) is NO LONGER HERE — it was moved to
    /// SampleData/SampleDataSeeder.cs and, instead of migration/HasData, is added via a runtime
    /// seed-runner (SampleDataSeedHostedService) that only runs in Development/Staging. See that file's description.
    /// </summary>
    public static void SeedSystemDefaults(this ModelBuilder modelBuilder)
    {
        modelBuilder.SeedRoles();
        modelBuilder.SeedPermissions();
        modelBuilder.SeedStatuses();
        modelBuilder.SeedOrganizations();
        modelBuilder.SeedMenus();
        modelBuilder.SeedAdminAccount();
    }
}
