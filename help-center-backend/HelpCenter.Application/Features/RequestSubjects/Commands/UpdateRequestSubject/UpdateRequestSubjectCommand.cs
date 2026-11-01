using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

public class UpdateRequestSubjectCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
    public string? SourceLocale { get; set; }
    public List<LocalizedNameInput>? Translations { get; set; }
}
