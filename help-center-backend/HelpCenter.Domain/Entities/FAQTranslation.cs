using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities;

public class FAQTranslation : BaseEntity
{
    public Guid FAQId { get; set; }
    public virtual FAQ FAQ { get; set; } = null!;
    public string Locale { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
