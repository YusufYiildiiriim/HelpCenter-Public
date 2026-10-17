using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class AdminAccountSeed
{
    private const string DefaultPasswordHash = "$2a$11$pCDJIRu1tH/Ir19q7P0fv.1oiPQ4SLP6vOEYU5LjinQtMzn5eL3f2";

    public static void SeedAdminAccount(this ModelBuilder modelBuilder)
    {
        // 1. Admin Account
        modelBuilder.Entity<Account>().HasData(
            new Account
            {
                PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1),
                Email = "admin@helpcenter.com",
                Username = "admin.user",
                FirstName = "Admin",
                LastName = "User",
                Password = DefaultPasswordHash,
                PhoneNumber = "+90 000 000 0000",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );

        // 2. Admin User
        modelBuilder.Entity<User>().HasData(
            new User { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), AccountId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // 3. Admin UserRole Mapping (User 1 -> Role 1 / Admin)
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), UserId = SeedIdentity.FromLegacyInt(1), RoleId = SeedIdentity.FromLegacyInt(1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
