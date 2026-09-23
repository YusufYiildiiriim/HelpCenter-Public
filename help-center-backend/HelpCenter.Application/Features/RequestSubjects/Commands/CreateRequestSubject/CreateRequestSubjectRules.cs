using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

/// <summary>
/// Rules belonging only to the request-subject-creation slice.
/// </summary>
public class CreateRequestSubjectRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRequestSubjectRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the subject name is unique.
    /// </summary>
    public async Task SubjectNameShouldBeUniqueAsync(string name, int? currentSubjectId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<RequestSubject>()
            .FindAsync(s => s.Name == name && (currentSubjectId == null || s.Id != currentSubjectId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new RequestSubjectNameAlreadyExistsException();
        }
    }
}
