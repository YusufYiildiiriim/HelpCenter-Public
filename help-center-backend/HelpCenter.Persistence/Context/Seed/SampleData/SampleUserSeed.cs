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
            Id = 2,
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
            Id = 3,
            Email = "agent2@helpcenter.com",
            Username = "agent2.user",
            FirstName = "Berna",
            LastName = "KisittiTemsilci",
            Password = DefaultPasswordHash,
            PhoneNumber = "+90 532 000 0003",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        // Expert (Module technical expert - MOD-1 Accounting and MOD-4 API)
        new Account
        {
            Id = 4,
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
        new User { Id = 2, AccountId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new User { Id = 3, AccountId = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new User { Id = 4, AccountId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<UserRole> BuildUserRoles() =>
    [
        new UserRole { Id = 2, UserId = 2, RoleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Agent 1 -> Agent
        new UserRole { Id = 3, UserId = 3, RoleId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }, // Agent 2 -> Agent
        new UserRole { Id = 4, UserId = 4, RoleId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }  // Expert -> Expert
    ];

    // Expert assignments: User 4 -> Module 1 & Module 4
    public static IReadOnlyList<ModuleExpert> BuildModuleExperts() =>
    [
        new ModuleExpert { Id = 1, ModuleId = 1, UserId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ModuleExpert { Id = 2, ModuleId = 4, UserId = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];

    public static IReadOnlyList<UserProject> BuildUserProjects() =>
    [
        new UserProject { Id = 1, UserId = 1, ProjectId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { Id = 2, UserId = 1, ProjectId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { Id = 3, UserId = 2, ProjectId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { Id = 4, UserId = 2, ProjectId = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new UserProject { Id = 5, UserId = 3, ProjectId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
    ];
}
