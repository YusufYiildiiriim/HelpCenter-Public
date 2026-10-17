using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

public class CreateGuideCommand : IRequest<bool>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FileUpload>? Files { get; set; }
    private Guid? _previousGuideId;
    public Guid? PreviousGuideId
    {
        get => _previousGuideId;
        set => _previousGuideId = (value == Guid.Empty ? null : value);
    }
    public Guid? ModuleId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? YoutubeUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
}
