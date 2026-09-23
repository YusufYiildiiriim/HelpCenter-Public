namespace HelpCenter.Application.Features.Faqs.Queries.GetFaqs;

public class FaqDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
