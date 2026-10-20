using HelpCenter.Domain.Entities;

namespace HelpCenter.Persistence.Context.Seed.SampleData;

/// <summary>
/// Sample staff (Agent/Expert) data. No longer embedded into migrations via
/// ModelBuilder.HasData() — this class only produces plain object lists; actual insertion
/// is done by SampleDataSeeder at runtime (only in Development/Staging).
/// </summary>
public static class SampleUserSeed
{
    private const string DefaultPasswordHash = "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2";

    // Id: 2..4 - Note: Id 1 (Admin) is seeded in AdminAccountSeed (SystemDefaults).
    public static IReadOnlyList<Account> BuildStaffAccounts() =>
    [
        // Agent 1 (Unrestricted customer representative)
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2),
            Email = "agent1@helpcenter.com",
            Username = "agent1.user",
            FirstName = "Ali",
            LastName = "Temsilci",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 532 000 0002",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Agent 2 (Restricted customer representative - Only PRJ-1 and MOD-1)
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3),
            Email = "agent2@helpcenter.com",
            Username = "agent2.user",
            FirstName = "Berna",
            LastName = "Kısıtlı Temsilci",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 532 000 0003",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Expert (Module technical expert - MOD-1 Accounting and MOD-4 API)
        new Account
        {
            PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4),
            Email = "expert@helpcenter.com",
            Username = "expert.user",
            FirstName = "Erol",
            LastName = "Uzman",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 532 000 0004",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        }
    ];

    public static IReadOnlyList<User> BuildUsers() =>
    [
        new User { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), AccountId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new User { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), AccountId = SeedIdentity.FromLegacyInt(3), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new User { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), AccountId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<UserRole> BuildUserRoles() =>
    [
        new UserRole { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), UserId = SeedIdentity.FromLegacyInt(2), RoleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Agent 1 -> Agent
        new UserRole { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), UserId = SeedIdentity.FromLegacyInt(3), RoleId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Agent 2 -> Agent
        new UserRole { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), UserId = SeedIdentity.FromLegacyInt(4), RoleId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }  // Expert -> Expert
    ];

    // Expert assignments: User 4 -> Module 1 & Module 4
    public static IReadOnlyList<ModuleExpert> BuildModuleExperts() =>
    [
        new ModuleExpert { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), ModuleId = SeedIdentity.FromLegacyInt(1), UserId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ModuleExpert { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), ModuleId = SeedIdentity.FromLegacyInt(4), UserId = SeedIdentity.FromLegacyInt(4), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<UserProject> BuildUserProjects() =>
    [
        new UserProject { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), UserId = SeedIdentity.FromLegacyInt(1), ProjectId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), UserId = SeedIdentity.FromLegacyInt(1), ProjectId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), UserId = SeedIdentity.FromLegacyInt(2), ProjectId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), UserId = SeedIdentity.FromLegacyInt(2), ProjectId = SeedIdentity.FromLegacyInt(2), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), UserId = SeedIdentity.FromLegacyInt(3), ProjectId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];
}
