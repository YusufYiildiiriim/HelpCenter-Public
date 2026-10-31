using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities;

public class GuideTranslationDocument : BaseEntity
{
    public Guid GuideTranslationId { get; set; }
    public virtual GuideTranslation GuideTranslation { get; set; } = null!;
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
