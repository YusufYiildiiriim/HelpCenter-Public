namespace HelpCenter.Domain.Entities
{
    public class UserProject : BaseEntity
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
