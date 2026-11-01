namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;

public class RequestSubjectDto
{
    public Guid Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public Guid RowVersion { get; set; }
    public string? SourceLocale { get; set; }
    public List<HelpCenter.Application.Common.Models.LocalizedNameInput> Translations { get; set; } = new();
}
