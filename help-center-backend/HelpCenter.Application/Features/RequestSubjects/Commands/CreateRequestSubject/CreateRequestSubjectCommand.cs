using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

public class CreateRequestSubjectCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
