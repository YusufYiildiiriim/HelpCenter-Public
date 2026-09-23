using HelpCenter.Domain.Enums;

namespace HelpCenter.Domain.Entities
{
    public class RequestHistory : BaseEntity
    {
        public int RequestId { get; set; }
        public virtual CustomerRequest Request { get; set; } = null!;

        public int ActorAccountId { get; set; }
        public virtual Account ActorAccount { get; set; } = null!;

        public RequestActionType Action { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Note { get; set; }
    }
}
