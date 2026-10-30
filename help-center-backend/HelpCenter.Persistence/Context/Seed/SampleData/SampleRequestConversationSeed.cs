using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Domain.Constants;

namespace HelpCenter.Persistence.Context.Seed.SampleData;

/// <summary>
/// Sample request/conversation data. No longer embedded into migrations via
/// ModelBuilder.HasData() — this class only produces plain object lists; actual insertion
/// is done by SampleDataSeeder at runtime (only in Development/Staging).
/// </summary>
public static class SampleRequestConversationSeed
{
    public static IReadOnlyList<Conversation> BuildConversations() =>
    [
        new Conversation { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), Title = "Acme Fatura Entegrasyon Sorunu", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        new Conversation { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), Title = "Beta API Token Hatası", IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
        new Conversation { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), Title = "Acme Stok Güncelleme Talebi", IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<CustomerRequest> BuildCustomerRequests() =>
    [
        new CustomerRequest
        {
            PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1),
            TicketId = "TKT-ACME-001",
            CustomerId = SeedIdentity.FromLegacyInt(1), // Ahmet (Acme)
            ModuleId = SeedIdentity.FromLegacyInt(1),   // Accounting & Invoicing
            RequestSubjectId = SeedIdentity.FromLegacyInt(1),
            Title = "Acme Fatura Entegrasyon Sorunu",
            Description = "E-Fatura gönderiminde 500 Internal Server Error hatası alınıyor.",
            Priority = RequestPriority.High,
            StatusId = RequestStatusConstants.UnderTechnicalReview,
            AssignedUserId = SeedIdentity.FromLegacyInt(2), // Agent 1
            CurrentExpertId = SeedIdentity.FromLegacyInt(4), // Expert (User 4)
            ConversationId = SeedIdentity.FromLegacyInt(1),
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new CustomerRequest
        {
            PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2),
            TicketId = "TKT-BETA-002",
            CustomerId = SeedIdentity.FromLegacyInt(3), // Mehmet (Beta)
            ModuleId = SeedIdentity.FromLegacyInt(4),   // API & Integration
            RequestSubjectId = SeedIdentity.FromLegacyInt(3),
            Title = "Beta API Token Hatası",
            Description = "REST API bearer token süresi beklenenden erken doluyor.",
            Priority = RequestPriority.Medium,
            StatusId = RequestStatusConstants.WaitingForReply,
            AssignedUserId = SeedIdentity.FromLegacyInt(2), // Agent 1
            ConversationId = SeedIdentity.FromLegacyInt(2),
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        },
        new CustomerRequest
        {
            PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3),
            TicketId = "TKT-ACME-003",
            CustomerId = SeedIdentity.FromLegacyInt(2), // Ayse (Acme)
            ModuleId = SeedIdentity.FromLegacyInt(2),   // Order & Stock
            RequestSubjectId = SeedIdentity.FromLegacyInt(2),
            Title = "Acme Stok Güncelleme Talebi",
            Description = "Depo stokları güncellendi, kontroller tamamlandı.",
            Priority = RequestPriority.Low,
            StatusId = RequestStatusConstants.Completed,
            AssignedUserId = SeedIdentity.FromLegacyInt(2), // Agent 1
            ConversationId = SeedIdentity.FromLegacyInt(3),
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc)
        }
    ];

    public static IReadOnlyList<ConversationParticipant> BuildConversationParticipants() =>
    [
        // Conversation 1 (Acme / Req 1)
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), ConversationId = SeedIdentity.FromLegacyInt(1), CustomerId = SeedIdentity.FromLegacyInt(1), Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), ConversationId = SeedIdentity.FromLegacyInt(1), UserId = SeedIdentity.FromLegacyInt(2), Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), ConversationId = SeedIdentity.FromLegacyInt(1), UserId = SeedIdentity.FromLegacyInt(4), Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

        // Conversation 2 (Beta / Req 2)
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), ConversationId = SeedIdentity.FromLegacyInt(2), CustomerId = SeedIdentity.FromLegacyInt(3), Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), ConversationId = SeedIdentity.FromLegacyInt(2), UserId = SeedIdentity.FromLegacyInt(2), Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },

        // Conversation 3 (Acme / Req 3)
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(6), Id = SeedIdentity.FromLegacyInt(6), ConversationId = SeedIdentity.FromLegacyInt(3), CustomerId = SeedIdentity.FromLegacyInt(2), Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) },
        new ConversationParticipant { PublicId = SeedIdentity.FromLegacyInt(7), Id = SeedIdentity.FromLegacyInt(7), ConversationId = SeedIdentity.FromLegacyInt(3), UserId = SeedIdentity.FromLegacyInt(2), Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc), IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<CustomerRequestMessage> BuildCustomerRequestMessages() =>
    [
        // Req 1 (Acme): 1 Public customer + 1 Public agent + 1 InternalNote expert
        new CustomerRequestMessage { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), ConversationId = SeedIdentity.FromLegacyInt(1), SenderParticipantId = SeedIdentity.FromLegacyInt(1), MessageText = "Fatura keserken 500 hatası alıyoruz.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
        new CustomerRequestMessage { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), ConversationId = SeedIdentity.FromLegacyInt(1), SenderParticipantId = SeedIdentity.FromLegacyInt(2), MessageText = "Talebinizi aldık, teknik uzmanımıza yönlendirdim.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 15, 0, DateTimeKind.Utc) },
        new CustomerRequestMessage { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), ConversationId = SeedIdentity.FromLegacyInt(1), SenderParticipantId = SeedIdentity.FromLegacyInt(3), MessageText = "[İÇ NOT] EF Core veritabanı kilitlenme logları inceleniyor. Müşteri tarafında ek aksiyon gerekmiyor.", Type = MessageType.InternalNote, IsRead = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 30, 0, DateTimeKind.Utc) },

        // Req 2 (Beta): 1 Public customer
        new CustomerRequestMessage { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), ConversationId = SeedIdentity.FromLegacyInt(2), SenderParticipantId = SeedIdentity.FromLegacyInt(4), MessageText = "REST API token 1 saat sonra geçersiz oluyor.", Type = MessageType.Public, IsRead = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 11, 0, 0, DateTimeKind.Utc) },

        // Req 3 (Acme): 1 Public customer
        new CustomerRequestMessage { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), ConversationId = SeedIdentity.FromLegacyInt(3), SenderParticipantId = SeedIdentity.FromLegacyInt(6), MessageText = "Stok listesi başarıyla güncellendi, teşekkürler.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 14, 0, 0, DateTimeKind.Utc) }
    ];

    public static IReadOnlyList<RequestHistory> BuildRequestHistories() =>
    [
        new RequestHistory { PublicId = SeedIdentity.FromLegacyInt(1), Id = SeedIdentity.FromLegacyInt(1), RequestId = SeedIdentity.FromLegacyInt(1), ActorAccountId = SeedIdentity.FromLegacyInt(5), Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
        new RequestHistory { PublicId = SeedIdentity.FromLegacyInt(2), Id = SeedIdentity.FromLegacyInt(2), RequestId = SeedIdentity.FromLegacyInt(1), ActorAccountId = SeedIdentity.FromLegacyInt(2), Action = RequestActionType.Assigned, Note = "Uzmana Yönlendirildi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 20, 0, DateTimeKind.Utc) },
        new RequestHistory { PublicId = SeedIdentity.FromLegacyInt(3), Id = SeedIdentity.FromLegacyInt(3), RequestId = SeedIdentity.FromLegacyInt(2), ActorAccountId = SeedIdentity.FromLegacyInt(7), Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 11, 0, 0, DateTimeKind.Utc) },
        new RequestHistory { PublicId = SeedIdentity.FromLegacyInt(4), Id = SeedIdentity.FromLegacyInt(4), RequestId = SeedIdentity.FromLegacyInt(3), ActorAccountId = SeedIdentity.FromLegacyInt(6), Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 14, 0, 0, DateTimeKind.Utc) },
        new RequestHistory { PublicId = SeedIdentity.FromLegacyInt(5), Id = SeedIdentity.FromLegacyInt(5), RequestId = SeedIdentity.FromLegacyInt(3), ActorAccountId = SeedIdentity.FromLegacyInt(2), Action = RequestActionType.StatusChanged, Note = "Talep Kapatıldı", IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 16, 0, 0, DateTimeKind.Utc) }
    ];
}
