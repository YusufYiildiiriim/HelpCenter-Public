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
            new RolePermission { PublicId = Guid.Empty, Id = 1, RoleId = 1, ResourceKey = AppResources.Dashboard, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 2, RoleId = 1, ResourceKey = AppResources.Requests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 3, RoleId = 1, ResourceKey = AppResources.Companies, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 4, RoleId = 1, ResourceKey = AppResources.Users, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 5, RoleId = 1, ResourceKey = AppResources.FAQ, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 6, RoleId = 1, ResourceKey = AppResources.Guide, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 8, RoleId = 1, ResourceKey = AppResources.Roles, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 9, RoleId = 1, ResourceKey = AppResources.Modules, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 10, RoleId = 1, ResourceKey = AppResources.Subjects, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 11, RoleId = 1, ResourceKey = AppResources.Statuses, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 12, RoleId = 1, ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 13, RoleId = 1, ResourceKey = AppResources.Customers, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 14, RoleId = 1, ResourceKey = AppResources.CustomerRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 15, RoleId = 1, ResourceKey = AppResources.Projects, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 16, RoleId = 1, ResourceKey = AppResources.OrganizationSettings, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Agent Permissions (RoleId: 2) - Requests, AssignedRequests, Companies, Dashboard (Restricted Fields)
            new RolePermission { PublicId = Guid.Empty, Id = 20, RoleId = 2, ResourceKey = AppResources.Requests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 21, RoleId = 2, ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 22, RoleId = 2, ResourceKey = AppResources.Companies, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermission { PublicId = Guid.Empty, Id = 23, RoleId = 2, ResourceKey = AppResources.Dashboard, AllowedFieldsJson = "[\"statusDistribution\",\"dailyTrend\"]", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Customer Permissions (RoleId: 3) - CustomerRequests
            new RolePermission { PublicId = Guid.Empty, Id = 30, RoleId = 3, ResourceKey = AppResources.CustomerRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Expert Permissions (RoleId: 4) - AssignedRequests
            new RolePermission { PublicId = Guid.Empty, Id = 40, RoleId = 4, ResourceKey = AppResources.AssignedRequests, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // 2. RolePermissionAction Seeds
        modelBuilder.Entity<RolePermissionAction>().HasData(
            // Admin Actions (RolePermissionId: 1..16 -> Read, Create, Update, Delete)
            new RolePermissionAction { PublicId = Guid.Empty, Id = 1, RolePermissionId = 1, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 2, RolePermissionId = 2, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 3, RolePermissionId = 2, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 4, RolePermissionId = 2, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 5, RolePermissionId = 2, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 6, RolePermissionId = 3, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 7, RolePermissionId = 3, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 8, RolePermissionId = 3, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 9, RolePermissionId = 3, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 10, RolePermissionId = 4, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 11, RolePermissionId = 4, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 12, RolePermissionId = 4, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 13, RolePermissionId = 4, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 14, RolePermissionId = 8, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 15, RolePermissionId = 8, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 16, RolePermissionId = 8, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 17, RolePermissionId = 8, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 18, RolePermissionId = 9, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 19, RolePermissionId = 9, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 20, RolePermissionId = 9, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 21, RolePermissionId = 9, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 22, RolePermissionId = 15, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 23, RolePermissionId = 15, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 24, RolePermissionId = 15, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 25, RolePermissionId = 15, Action = PermissionActions.Delete, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 26, RolePermissionId = 16, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 27, RolePermissionId = 16, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Agent Actions
            new RolePermissionAction { PublicId = Guid.Empty, Id = 50, RolePermissionId = 20, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 51, RolePermissionId = 20, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 52, RolePermissionId = 21, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 53, RolePermissionId = 22, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 54, RolePermissionId = 23, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Customer Actions
            new RolePermissionAction { PublicId = Guid.Empty, Id = 70, RolePermissionId = 30, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 71, RolePermissionId = 30, Action = PermissionActions.Create, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 72, RolePermissionId = 30, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

            // Expert Actions
            new RolePermissionAction { PublicId = Guid.Empty, Id = 80, RolePermissionId = 40, Action = PermissionActions.Read, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new RolePermissionAction { PublicId = Guid.Empty, Id = 81, RolePermissionId = 40, Action = PermissionActions.Update, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
