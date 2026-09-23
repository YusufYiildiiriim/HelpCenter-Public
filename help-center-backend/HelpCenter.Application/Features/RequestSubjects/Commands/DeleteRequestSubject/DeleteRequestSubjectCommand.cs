using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;

public record DeleteRequestSubjectCommand(int Id) : IRequest<bool>;
