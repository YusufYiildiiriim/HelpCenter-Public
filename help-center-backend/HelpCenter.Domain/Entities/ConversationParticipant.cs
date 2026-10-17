using HelpCenter.Domain.Enums;

namespace HelpCenter.Domain.Entities
{
    public class ConversationParticipant : BaseEntity
    {
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public ParticipantType Type { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CustomerRequestMessage> Messages { get; set; } = new HashSet<CustomerRequestMessage>();
    }
}
