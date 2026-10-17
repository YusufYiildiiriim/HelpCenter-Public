namespace HelpCenter.Domain.Entities
{
    public class CompanyModule : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        public Guid ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;
    }
}
