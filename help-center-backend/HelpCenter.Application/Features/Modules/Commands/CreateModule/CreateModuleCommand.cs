using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Modules.Commands.CreateModule;

public class CreateModuleCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Guid>? ExpertUserIds { get; set; }
    public string? SourceLocale { get; set; }
    public List<LocalizedNameInput> Translations { get; set; } = new();
}
