namespace HelpCenter.Domain.Entities
{
    public class CompanyModule : BaseEntity
    {
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;
    }
}
