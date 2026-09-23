using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class RoleSeed
{
    public static void SeedRoles(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Süper Admin - Tam yetki", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Role { Id = 2, Name = "Agent", Description = "Müşteri Temsilcisi - Talep ve firma okuma/yönetme", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Role { Id = 3, Name = "Customer", Description = "Son Kullanıcı - Talep oluşturma ve mesajlaşma", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Role { Id = 4, Name = "Expert", Description = "Yetkili Kişi - Modül uzmanı, yalnızca atanan talepleri inceler", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
