using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

/// <summary>
/// Rules belonging only to the remove-user-from-project slice.
/// </summary>
public static class RemoveUserFromProjectRules
{
    /// <summary>
    /// Verifies that the project-user assignment exists.
    /// </summary>
    public static void ProjectAssignmentShouldExist(UserProject? userProject)
    {
        if (userProject == null)
        {
            throw new ProjectAssignmentNotFoundException();
        }
    }
}
