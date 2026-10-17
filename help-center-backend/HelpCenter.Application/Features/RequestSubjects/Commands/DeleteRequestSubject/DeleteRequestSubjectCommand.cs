using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;

public record DeleteRequestSubjectCommand(Guid Id) : IRequest<bool>;
