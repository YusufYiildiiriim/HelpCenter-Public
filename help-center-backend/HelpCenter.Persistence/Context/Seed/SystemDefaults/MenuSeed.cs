using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class MenuSeed
{
    public static void SeedMenus(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { PublicId = Guid.Empty, Id = 15, Label = "Denetim Kayıtları", Icon = "ClipboardList", Route = "/admin/settings/audit-logs", ResourceKey = AppResources.AuditLogs, Order = 15, GroupTitle = null, CreatedAt = new DateTime(2026, 10, 16), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 1, Label = "İstatistikler", Icon = "BarChart3", Route = "/admin", ResourceKey = AppResources.Dashboard, Order = 1, GroupTitle = "Talep Takip", CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 2, Label = "Talepler", Icon = "ClipboardList", Route = "/admin/requests", ResourceKey = AppResources.Requests, Order = 2, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 3, Label = "Bana Atananlar", Icon = "Inbox", Route = "/admin/requests/assigned", ResourceKey = AppResources.AssignedRequests, Order = 3, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 4, Label = "Firmalar", Icon = "Building2", Route = "/admin/companies", ResourceKey = AppResources.Companies, Order = 4, GroupTitle = "Tanımlamalar", CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 5, Label = "Projeler", Icon = "FolderKanban", Route = "/admin/projects", ResourceKey = AppResources.Projects, Order = 5, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 6, Label = "Kullanıcılar", Icon = "Users", Route = "/admin/users", ResourceKey = AppResources.Users, Order = 6, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 7, Label = "SSS Yönetimi", Icon = "HelpCircle", Route = "/admin/ss", ResourceKey = AppResources.FAQ, Order = 7, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 8, Label = "Kullanım Kılavuzu", Icon = "BookOpen", Route = "/admin/guide", ResourceKey = AppResources.Guide, Order = 8, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 10, Label = "Rol ve Yetkiler", Icon = "ShieldCheck", Route = "/admin/settings/roles", ResourceKey = AppResources.Roles, Order = 10, GroupTitle = "Sistem Ayarları", CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 11, Label = "Departman Modülleri", Icon = "LayoutGrid", Route = "/admin/modules", ResourceKey = AppResources.Modules, Order = 11, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 12, Label = "Talep Konuları", Icon = "MessageSquare", Route = "/admin/subjects", ResourceKey = AppResources.Subjects, Order = 12, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 13, Label = "Talep Durumları", Icon = "LayoutList", Route = "/admin/statuses", ResourceKey = AppResources.Statuses, Order = 13, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true },
            new MenuItem { PublicId = Guid.Empty, Id = 14, Label = "Kurum Bilgileri", Icon = "Building2", Route = "/admin/settings/organization", ResourceKey = AppResources.OrganizationSettings, Order = 14, GroupTitle = null, CreatedAt = new DateTime(2026, 8, 1), IsActive = true }
        );
    }
}
