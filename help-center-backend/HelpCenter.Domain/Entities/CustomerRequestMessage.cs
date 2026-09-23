using HelpCenter.Domain.Enums;

namespace HelpCenter.Domain.Entities
{
    public class CustomerRequestMessage : BaseEntity
    {
        public int ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; } = null!;

        public int SenderParticipantId { get; set; }
        public virtual ConversationParticipant SenderParticipant { get; set; } = null!;

        public string MessageText { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public MessageType Type { get; set; } = MessageType.Public;

        public virtual ICollection<CustomerRequestMessageDocument> Documents { get; set; } = new List<CustomerRequestMessageDocument>();
    }
}
