namespace HelpCenter.Domain.Entities
{
    public class ModuleExpert : BaseEntity
    {
        public Guid ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
