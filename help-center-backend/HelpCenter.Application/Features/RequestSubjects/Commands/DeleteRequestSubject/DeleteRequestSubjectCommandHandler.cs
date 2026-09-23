using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;

public class DeleteRequestSubjectCommandHandler : IRequestHandler<DeleteRequestSubjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRequestSubjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteRequestSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _unitOfWork.Repository<RequestSubject>().GetAsync(request.Id, cancellationToken);
        DeleteRequestSubjectRules.SubjectShouldExist(subject);
        DeleteRequestSubjectRules.SubjectShouldNotBeLocked(subject!);

        subject!.MarkAsDeleted();
        await _unitOfWork.Repository<RequestSubject>().UpdateAsync(subject, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
