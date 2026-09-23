using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

public class UpdateRequestSubjectCommandHandler : IRequestHandler<UpdateRequestSubjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateRequestSubjectRules _rules;

    public UpdateRequestSubjectCommandHandler(IUnitOfWork unitOfWork, UpdateRequestSubjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateRequestSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _unitOfWork.Repository<RequestSubject>().GetAsync(request.Id, cancellationToken);
        UpdateRequestSubjectRules.SubjectShouldExist(subject);
        UpdateRequestSubjectRules.SubjectShouldNotBeLocked(subject!);

        await _rules.SubjectNameShouldBeUniqueAsync(request.Name, request.Id, cancellationToken);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(subject!, request.RowVersion);

        subject!.UpdateInfo(request.Name, request.Description, null, request.IsActive);

        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
