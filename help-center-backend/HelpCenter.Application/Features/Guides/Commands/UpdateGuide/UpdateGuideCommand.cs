using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

public class UpdateGuideCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SourceLocale { get; set; }
    public List<LocalizedTextInput>? Translations { get; set; }
    public List<FileUpload>? Files { get; set; }
    public List<FileUpload>? EnglishFiles { get; set; }
    private Guid? _previousGuideId;
    public Guid? PreviousGuideId
    {
        get => _previousGuideId;
        set => _previousGuideId = (value == Guid.Empty ? null : value);
    }
    public Guid? ModuleId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? YoutubeUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public Guid RowVersion { get; set; }
}
