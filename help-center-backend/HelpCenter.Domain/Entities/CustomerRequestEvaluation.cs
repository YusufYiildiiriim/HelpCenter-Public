namespace HelpCenter.Domain.Entities
{
    public class CustomerRequestEvaluation : BaseEntity
    {
        public int CustomerRequestId { get; set; }
        public virtual CustomerRequest CustomerRequest { get; set; } = null!;

        public string Note { get; set; } = string.Empty;
        public int Rating { get; set; }

        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;

        public int CustomerUserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}
