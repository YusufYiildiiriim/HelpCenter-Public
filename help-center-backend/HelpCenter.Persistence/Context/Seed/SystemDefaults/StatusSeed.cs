using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

public static class StatusSeed
{
    public static void SeedStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerRequestStatus>().HasData(
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = RequestStatusConstants.WaitingForReply, Name = "Cevap Bekliyor", Description = "Kullanıcıdan gelen talep cevaplanmayı bekliyor.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = RequestStatusConstants.Replied, Name = "Cevaplandı", Description = "Talep bir temsilci tarafından cevaplandı.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = RequestStatusConstants.Completed, Name = "Tamamlandı", Description = "Talep başarıyla kapatıldı.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new CustomerRequestStatus { PublicId = Guid.Empty, Id = RequestStatusConstants.UnderTechnicalReview, Name = "Teknik İncelemede", Description = "Talep bir uzman tarafından inceleniyor.", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );
    }
}
