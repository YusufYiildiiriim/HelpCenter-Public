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
                Id = 1,
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
            new User { Id = 1, AccountId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // 3. Admin UserRole Mapping (User 1 -> Role 1 / Admin)
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { Id = 1, UserId = 1, RoleId = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
