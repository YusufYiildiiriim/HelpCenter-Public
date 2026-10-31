using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities;

public class GuideTranslation : BaseEntity
{
    public Guid GuideId { get; set; }
    public virtual Guide Guide { get; set; } = null!;
    public string Locale { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<GuideTranslationDocument> Documents { get; set; } = new HashSet<GuideTranslationDocument>();
}
