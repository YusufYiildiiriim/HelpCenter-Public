using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;

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
        new Conversation { Id = 1, Title = "Acme Fatura Entegrasyon Sorunu", IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new Conversation { Id = 2, Title = "Beta API Token Hatası", IsActive = true, CreatedAt = new DateTime(2024, 1, 2) },
        new Conversation { Id = 3, Title = "Acme Stok Güncelleme Talebi", IsActive = true, CreatedAt = new DateTime(2024, 1, 3) }
    ];

    public static IReadOnlyList<CustomerRequest> BuildCustomerRequests() =>
    [
        new CustomerRequest
        {
            Id = 1,
            TicketId = "TKT-ACME-001",
            CustomerId = 1, // Ahmet (Acme)
            ModuleId = 1,   // Accounting & Invoicing
            RequestSubjectId = 1,
            Title = "Acme Fatura Entegrasyon Sorunu",
            Description = "E-Fatura gönderiminde 500 Internal Server Error hatası alınıyor.",
            Priority = RequestPriority.High,
            StatusId = 4,   // In Technical Review
            AssignedUserId = 2, // Agent 1
            CurrentExpertId = 4, // Expert (User 4)
            ConversationId = 1,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        },
        new CustomerRequest
        {
            Id = 2,
            TicketId = "TKT-BETA-002",
            CustomerId = 3, // Mehmet (Beta)
            ModuleId = 4,   // API & Integration
            RequestSubjectId = 3,
            Title = "Beta API Token Hatası",
            Description = "REST API bearer token süresi beklenenden erken doluyor.",
            Priority = RequestPriority.Medium,
            StatusId = 1,   // Awaiting Reply
            AssignedUserId = 2, // Agent 1
            ConversationId = 2,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 2)
        },
        new CustomerRequest
        {
            Id = 3,
            TicketId = "TKT-ACME-003",
            CustomerId = 2, // Ayse (Acme)
            ModuleId = 2,   // Order & Stock
            RequestSubjectId = 2,
            Title = "Acme Stok Güncelleme Talebi",
            Description = "Depo stokları güncellendi, kontroller tamamlandı.",
            Priority = RequestPriority.Low,
            StatusId = 3,   // Completed
            AssignedUserId = 2, // Agent 1
            ConversationId = 3,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 3)
        }
    ];

    public static IReadOnlyList<ConversationParticipant> BuildConversationParticipants() =>
    [
        // Conversation 1 (Acme / Req 1)
        new ConversationParticipant { Id = 1, ConversationId = 1, CustomerId = 1, Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ConversationParticipant { Id = 2, ConversationId = 1, UserId = 2, Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
        new ConversationParticipant { Id = 3, ConversationId = 1, UserId = 4, Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 1), IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },

        // Conversation 2 (Beta / Req 2)
        new ConversationParticipant { Id = 4, ConversationId = 2, CustomerId = 3, Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 2), IsActive = true, CreatedAt = new DateTime(2024, 1, 2) },
        new ConversationParticipant { Id = 5, ConversationId = 2, UserId = 2, Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 2), IsActive = true, CreatedAt = new DateTime(2024, 1, 2) },

        // Conversation 3 (Acme / Req 3)
        new ConversationParticipant { Id = 6, ConversationId = 3, CustomerId = 2, Type = ParticipantType.Customer, JoinedAt = new DateTime(2024, 1, 3), IsActive = true, CreatedAt = new DateTime(2024, 1, 3) },
        new ConversationParticipant { Id = 7, ConversationId = 3, UserId = 2, Type = ParticipantType.Agent, JoinedAt = new DateTime(2024, 1, 3), IsActive = true, CreatedAt = new DateTime(2024, 1, 3) }
    ];

    public static IReadOnlyList<CustomerRequestMessage> BuildCustomerRequestMessages() =>
    [
        // Req 1 (Acme): 1 Public customer + 1 Public agent + 1 InternalNote expert
        new CustomerRequestMessage { Id = 1, ConversationId = 1, SenderParticipantId = 1, MessageText = "Fatura keserken 500 hatası alıyoruz.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0) },
        new CustomerRequestMessage { Id = 2, ConversationId = 1, SenderParticipantId = 2, MessageText = "Talebinizi aldık, teknik uzmanımıza yönlendirdim.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 15, 0) },
        new CustomerRequestMessage { Id = 3, ConversationId = 1, SenderParticipantId = 3, MessageText = "[İÇ NOT] EF Core veritabanı kilitlenme logları inceleniyor. Müşteri tarafında ek aksiyon gerekmiyor.", Type = MessageType.InternalNote, IsRead = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 30, 0) },

        // Req 2 (Beta): 1 Public customer
        new CustomerRequestMessage { Id = 4, ConversationId = 2, SenderParticipantId = 4, MessageText = "REST API token 1 saat sonra geçersiz oluyor.", Type = MessageType.Public, IsRead = false, IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 11, 0, 0) },

        // Req 3 (Acme): 1 Public customer
        new CustomerRequestMessage { Id = 5, ConversationId = 3, SenderParticipantId = 6, MessageText = "Stok listesi başarıyla güncellendi, teşekkürler.", Type = MessageType.Public, IsRead = true, IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 14, 0, 0) }
    ];

    public static IReadOnlyList<RequestHistory> BuildRequestHistories() =>
    [
        new RequestHistory { Id = 1, RequestId = 1, ActorAccountId = 5, Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0) },
        new RequestHistory { Id = 2, RequestId = 1, ActorAccountId = 2, Action = RequestActionType.Assigned, Note = "Uzmana Yönlendirildi", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 10, 20, 0) },
        new RequestHistory { Id = 3, RequestId = 2, ActorAccountId = 7, Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 2, 11, 0, 0) },
        new RequestHistory { Id = 4, RequestId = 3, ActorAccountId = 6, Action = RequestActionType.Created, Note = "Talep Oluşturuldu", IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 14, 0, 0) },
        new RequestHistory { Id = 5, RequestId = 3, ActorAccountId = 2, Action = RequestActionType.StatusChanged, Note = "Talep Kapatıldı", IsActive = true, CreatedAt = new DateTime(2024, 1, 3, 16, 0, 0) }
    ];
}
