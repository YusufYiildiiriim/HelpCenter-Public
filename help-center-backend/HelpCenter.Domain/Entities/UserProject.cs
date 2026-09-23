namespace HelpCenter.Domain.Entities
{
    public class UserProject : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
