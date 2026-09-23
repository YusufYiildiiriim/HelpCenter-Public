using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

public class UpdateRequestSubjectCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public byte[]? RowVersion { get; set; }
}
