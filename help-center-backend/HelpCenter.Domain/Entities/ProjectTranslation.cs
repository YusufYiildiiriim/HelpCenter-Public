namespace HelpCenter.Domain.Entities;

public class ProjectTranslation : BaseEntity
{
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
