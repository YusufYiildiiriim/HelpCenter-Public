using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;

/// <summary>
/// Rules belonging only to the request-subject-deletion slice.
/// </summary>
public static class DeleteRequestSubjectRules
{
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
}
