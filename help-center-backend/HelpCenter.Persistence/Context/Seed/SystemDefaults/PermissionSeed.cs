using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class PermissionSeed
{
    public static void SeedPermissions(this ModelBuilder modelBuilder)
    {
        // 1. RolePermission Seeds
        modelBuilder.Entity<RolePermission>().HasData(
            // Admin Permissions (RoleId: 1) - All Resources
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Dashboard, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Requests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Companies, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Users, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.FAQ, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(6), Id = SeedIdentity.FromLegacyInt(6), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Guide, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(8), Id = SeedIdentity.FromLegacyInt(8), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Roles, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(9), Id = SeedIdentity.FromLegacyInt(9), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Modules, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(10), Id = SeedIdentity.FromLegacyInt(10), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Subjects, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(11), Id = SeedIdentity.FromLegacyInt(11), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Statuses, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(12), Id = SeedIdentity.FromLegacyInt(12), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(13), Id = SeedIdentity.FromLegacyInt(13), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Customers, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(14), Id = SeedIdentity.FromLegacyInt(14), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.CustomerRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(15), Id = SeedIdentity.FromLegacyInt(15), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.Projects, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(16), Id = SeedIdentity.FromLegacyInt(16), RoleId = SeedIdentity.FromLegacyInt(1), ResourceKey = AppResources.OrganizationSettings, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Agent Permissions (RoleId: 2) - Requests, AssignedRequests, Companies, Dashboard (Restricted Fields)
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(20), Id = SeedIdentity.FromLegacyInt(20), RoleId = SeedIdentity.FromLegacyInt(2), ResourceKey = AppResources.Requests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(21), Id = SeedIdentity.FromLegacyInt(21), RoleId = SeedIdentity.FromLegacyInt(2), ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(22), Id = SeedIdentity.FromLegacyInt(22), RoleId = SeedIdentity.FromLegacyInt(2), ResourceKey = AppResources.Companies, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(23), Id = SeedIdentity.FromLegacyInt(23), RoleId = SeedIdentity.FromLegacyInt(2), ResourceKey = AppResources.Dashboard, AllowedFieldsJson = "[\"statusDistribution\",\"dailyTrend\"]", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Customer Permissions (RoleId: 3) - CustomerRequests
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(30), Id = SeedIdentity.FromLegacyInt(30), RoleId = SeedIdentity.FromLegacyInt(3), ResourceKey = AppResources.CustomerRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Expert Permissions (RoleId: 4) - AssignedRequests
            new RolePermission { PublicId = SeedIdentity.FromLegacyInt(40), Id = SeedIdentity.FromLegacyInt(40), RoleId = SeedIdentity.FromLegacyInt(4), ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // 2. RolePermissionAction Seeds
        modelBuilder.Entity<RolePermissionAction>().HasData(
            // Admin Actions (RolePermissionId: 1..16 -> Read, Create, Update, Delete)
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), RolePermissionId = SeedIdentity.FromLegacyInt(1), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), RolePermissionId = SeedIdentity.FromLegacyInt(2), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), RolePermissionId = SeedIdentity.FromLegacyInt(2), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), RolePermissionId = SeedIdentity.FromLegacyInt(2), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), RolePermissionId = SeedIdentity.FromLegacyInt(2), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(6), Id = SeedIdentity.FromLegacyInt(6), RolePermissionId = SeedIdentity.FromLegacyInt(3), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(7), Id = SeedIdentity.FromLegacyInt(7), RolePermissionId = SeedIdentity.FromLegacyInt(3), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(8), Id = SeedIdentity.FromLegacyInt(8), RolePermissionId = SeedIdentity.FromLegacyInt(3), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(9), Id = SeedIdentity.FromLegacyInt(9), RolePermissionId = SeedIdentity.FromLegacyInt(3), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(10), Id = SeedIdentity.FromLegacyInt(10), RolePermissionId = SeedIdentity.FromLegacyInt(4), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(11), Id = SeedIdentity.FromLegacyInt(11), RolePermissionId = SeedIdentity.FromLegacyInt(4), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(12), Id = SeedIdentity.FromLegacyInt(12), RolePermissionId = SeedIdentity.FromLegacyInt(4), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(13), Id = SeedIdentity.FromLegacyInt(13), RolePermissionId = SeedIdentity.FromLegacyInt(4), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(14), Id = SeedIdentity.FromLegacyInt(14), RolePermissionId = SeedIdentity.FromLegacyInt(8), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(15), Id = SeedIdentity.FromLegacyInt(15), RolePermissionId = SeedIdentity.FromLegacyInt(8), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(16), Id = SeedIdentity.FromLegacyInt(16), RolePermissionId = SeedIdentity.FromLegacyInt(8), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(17), Id = SeedIdentity.FromLegacyInt(17), RolePermissionId = SeedIdentity.FromLegacyInt(8), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(18), Id = SeedIdentity.FromLegacyInt(18), RolePermissionId = SeedIdentity.FromLegacyInt(9), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(19), Id = SeedIdentity.FromLegacyInt(19), RolePermissionId = SeedIdentity.FromLegacyInt(9), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(20), Id = SeedIdentity.FromLegacyInt(20), RolePermissionId = SeedIdentity.FromLegacyInt(9), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(21), Id = SeedIdentity.FromLegacyInt(21), RolePermissionId = SeedIdentity.FromLegacyInt(9), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(22), Id = SeedIdentity.FromLegacyInt(22), RolePermissionId = SeedIdentity.FromLegacyInt(15), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(23), Id = SeedIdentity.FromLegacyInt(23), RolePermissionId = SeedIdentity.FromLegacyInt(15), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(24), Id = SeedIdentity.FromLegacyInt(24), RolePermissionId = SeedIdentity.FromLegacyInt(15), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(25), Id = SeedIdentity.FromLegacyInt(25), RolePermissionId = SeedIdentity.FromLegacyInt(15), Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(26), Id = SeedIdentity.FromLegacyInt(26), RolePermissionId = SeedIdentity.FromLegacyInt(16), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(27), Id = SeedIdentity.FromLegacyInt(27), RolePermissionId = SeedIdentity.FromLegacyInt(16), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Agent Actions
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(50), Id = SeedIdentity.FromLegacyInt(50), RolePermissionId = SeedIdentity.FromLegacyInt(20), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(51), Id = SeedIdentity.FromLegacyInt(51), RolePermissionId = SeedIdentity.FromLegacyInt(20), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(52), Id = SeedIdentity.FromLegacyInt(52), RolePermissionId = SeedIdentity.FromLegacyInt(21), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(53), Id = SeedIdentity.FromLegacyInt(53), RolePermissionId = SeedIdentity.FromLegacyInt(22), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(54), Id = SeedIdentity.FromLegacyInt(54), RolePermissionId = SeedIdentity.FromLegacyInt(23), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Customer Actions
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(70), Id = SeedIdentity.FromLegacyInt(70), RolePermissionId = SeedIdentity.FromLegacyInt(30), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(71), Id = SeedIdentity.FromLegacyInt(71), RolePermissionId = SeedIdentity.FromLegacyInt(30), Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(72), Id = SeedIdentity.FromLegacyInt(72), RolePermissionId = SeedIdentity.FromLegacyInt(30), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Expert Actions
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(80), Id = SeedIdentity.FromLegacyInt(80), RolePermissionId = SeedIdentity.FromLegacyInt(40), Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermissionAction { PublicId = SeedIdentity.FromLegacyInt(81), Id = SeedIdentity.FromLegacyInt(81), RolePermissionId = SeedIdentity.FromLegacyInt(40), Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
