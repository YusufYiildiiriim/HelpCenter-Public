namespace HelpCenter.Domain.Entities;

public class RequestSubjectTranslation : BaseEntity
{
    public Guid RequestSubjectId { get; set; }
    public virtual RequestSubject RequestSubject { get; set; } = null!;
    public string Locale { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
