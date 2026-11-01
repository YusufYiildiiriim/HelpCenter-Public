namespace HelpCenter.Domain.Entities;

public class ModuleTranslation : BaseEntity
{
    public Guid ModuleId { get; set; }
    public virtual Module Module { get; set; } = null!;
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
