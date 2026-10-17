namespace HelpCenter.Domain.Entities
{
    public class ProjectModule : BaseEntity
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;
    }
}
