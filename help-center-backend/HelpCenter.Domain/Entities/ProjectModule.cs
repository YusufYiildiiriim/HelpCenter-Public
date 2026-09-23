namespace HelpCenter.Domain.Entities
{
    public class ProjectModule : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;
    }
}
