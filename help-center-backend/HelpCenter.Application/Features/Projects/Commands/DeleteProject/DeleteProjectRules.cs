using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// Rules belonging only to the project-deletion slice.
/// </summary>
public static class DeleteProjectRules
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
    /// Verifies that the project cannot be deleted while it has active companies.
    /// </summary>
    public static void ProjectShouldNotHaveActiveCompanies(Project project)
    {
        if (project.Companies.Any(c => !c.IsDeleted))
        {
            throw new ProjectHasActiveCompaniesException("Bu projeye bağlı firmalar olduğu için proje silinemez. Önce firmaların proje bağlantısını kaldırın.");
        }
    }
}
