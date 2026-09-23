using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

/// <summary>
/// Rules belonging only to the assign-user-to-project slice.
/// </summary>
public static class AssignUserToProjectRules
{
    /// <summary>
    /// Verifies that the project exists in the system.
    /// </summary>
    public static void ProjectShouldExist(Project? project)
    {
        if (project == null || project.IsDeleted)
        {
            throw new ProjectNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the user to be assigned to the project exists in the system.
    /// </summary>
    public static void UserShouldExist(User? user)
    {
        if (user == null || user.IsDeleted)
        {
            throw new UserNotFoundException();
        }
    }
}
