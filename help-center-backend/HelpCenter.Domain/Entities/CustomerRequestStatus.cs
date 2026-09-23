namespace HelpCenter.Domain.Entities
{
    public class CustomerRequestStatus : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }
    }
}
