namespace HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;

public class PublicGuideDto
{
    public Guid PublicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? YoutubeUrl { get; set; }
    public Guid? PreviousGuidePublicId { get; set; }
    public List<PublicGuideDocumentDto>? Documents { get; set; }
}

public class PublicGuideDocumentDto
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
