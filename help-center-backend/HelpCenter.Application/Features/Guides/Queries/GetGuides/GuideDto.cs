namespace HelpCenter.Application.Features.Guides.Queries.GetGuides;

public class GuideDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? PreviousGuideId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? YoutubeUrl { get; set; }
    public int DocumentCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public List<GuideDocumentDto>? Documents { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

public class GuideDocumentDto
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
