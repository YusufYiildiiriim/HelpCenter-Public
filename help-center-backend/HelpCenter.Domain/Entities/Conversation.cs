using HelpCenter.Domain.Enums;

namespace HelpCenter.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public string? Title { get; set; }

        public virtual ICollection<ConversationParticipant> Participants { get; set; } = new HashSet<ConversationParticipant>();
        public virtual ICollection<CustomerRequestMessage> Messages { get; set; } = new HashSet<CustomerRequestMessage>();

        public ConversationParticipant AddParticipant(int? customerId, int? userId, ParticipantType type)
        {
            var participant = Participants.FirstOrDefault(p =>
                (customerId.HasValue && p.CustomerId == customerId && p.Type == type) ||
                (userId.HasValue && p.UserId == userId && p.Type == type));

            if (participant == null)
            {
                participant = new ConversationParticipant
                {
                    ConversationId = this.Id,
                    CustomerId = customerId,
                    UserId = userId,
                    Type = type
                };
                Participants.Add(participant);
            }

            return participant;
        }

        public int MarkMessagesAsRead(bool readerIsAgent)
        {
            var unread = Messages
                .Where(m =>
                    !m.IsRead &&
                    !m.IsDeleted &&
                    (readerIsAgent
                        ? m.SenderParticipant.Type == ParticipantType.Customer
                        : m.SenderParticipant.Type != ParticipantType.Customer))
                .ToList();

            foreach (var message in unread)
                message.IsRead = true;

            return unread.Count;
        }
    }
}
