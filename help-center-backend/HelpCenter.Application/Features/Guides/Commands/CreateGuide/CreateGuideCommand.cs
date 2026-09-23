using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

public class CreateGuideCommand : IRequest<bool>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FileUpload>? Files { get; set; }
    private int? _previousGuideId;
    public int? PreviousGuideId
    {
        get => _previousGuideId;
        set => _previousGuideId = (value == 0 ? null : value);
    }
    public string Module { get; set; } = string.Empty;
    public string? YoutubeUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
}
