using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// Rules belonging only to the project-creation slice.
/// </summary>
public class CreateProjectRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
}
