using HelpCenter.Domain.Enums;

namespace HelpCenter.Domain.Entities
{
    public class ConversationParticipant : BaseEntity
    {
        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public ParticipantType Type { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CustomerRequestMessage> Messages { get; set; } = new HashSet<CustomerRequestMessage>();
    }
}
