namespace HelpCenter.Application.Features.Faqs.Queries.GetFaqs;

public class FaqDto
{
    public Guid Id { get; set; }
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SourceLocale { get; set; }
    public List<HelpCenter.Application.Common.Models.LocalizedTextInput> Translations { get; set; } = new();
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public Guid RowVersion { get; set; }
}
