using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
    public List<Guid>? ExpertUserIds { get; set; }
    public string? SourceLocale { get; set; }
    public List<LocalizedNameInput>? Translations { get; set; }
}
