namespace HelpCenter.Domain.Entities
{
    public class ModuleExpert : BaseEntity
    {
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
