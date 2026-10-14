using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class StatusSeed
{
    public static void SeedStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerRequestStatus>().HasData(
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = 1, Name = "Cevap Bekliyor", Description = "Kullanıcıdan gelen talep cevaplanmayı bekliyor.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = 2, Name = "Cevaplandı", Description = "Talep bir temsilci tarafından cevaplandı.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = 3, Name = "Tamamlandı", Description = "Talep başarıyla kapatıldı.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = 4, Name = "Teknik İncelemede", Description = "Talep bir uzman tarafından inceleniyor.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
