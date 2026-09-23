using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// Rules belonging only to the project-update slice.
/// </summary>
public class UpdateProjectRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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
    /// Verifies that the project name is unique.
    /// </summary>
    public async Task ProjectNameShouldBeUniqueAsync(string name, int? currentProjectId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Project>()
            .FindAsync(p => p.Name == name && (currentProjectId == null || p.Id != currentProjectId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new ProjectNameAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies that the project cannot be deactivated while it has active companies.
    /// </summary>
    public static void ProjectShouldNotHaveActiveCompaniesWhenDeactivating(Project project, bool newIsActive)
    {
        if (!newIsActive && project.Companies.Any(c => !c.IsDeleted))
        {
            throw new ProjectHasActiveCompaniesException("Bu projeye bağlı firmalar olduğu için proje pasife alınamaz. Önce firmaların proje bağlantısını kaldırın.");
        }
    }
}
