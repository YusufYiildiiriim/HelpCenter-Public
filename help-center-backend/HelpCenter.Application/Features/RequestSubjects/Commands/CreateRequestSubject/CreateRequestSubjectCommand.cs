using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

public class CreateRequestSubjectCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? SourceLocale { get; set; }
    public List<LocalizedNameInput> Translations { get; set; } = new();
}
