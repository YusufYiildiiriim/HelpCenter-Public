using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

/// <summary>
/// Rules belonging only to the request-subject-update slice.
/// </summary>
public class UpdateRequestSubjectRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRequestSubjectRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the request subject exists in the system.
    /// </summary>
    public static void SubjectShouldExist(RequestSubject? subject)
    {
        if (subject == null || subject.IsDeleted)
        {
            throw new RequestSubjectNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the request subject is not locked.
    /// </summary>
    public static void SubjectShouldNotBeLocked(RequestSubject subject)
    {
        if (subject.IsLocked)
        {
            throw new RequestSubjectLockedException();
        }
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
