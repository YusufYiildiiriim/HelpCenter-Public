using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Faqs.Commands.CreateFaq;

/// <summary>
/// Rules belonging only to the create FAQ slice.
/// </summary>
public class CreateFaqRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFaqRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the FAQ title is unique.
    /// </summary>
    public async Task FaqTitleShouldBeUniqueAsync(string title, int? currentFaqId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<FAQ>()
            .FindAsync(f => f.Title == title && (currentFaqId == null || f.Id != currentFaqId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new FaqTitleAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies the selected project exists in the system.
    /// </summary>
    public async Task ProjectShouldExistAsync(int? projectId, CancellationToken cancellationToken)
    {
        if (!projectId.HasValue) return;

        var project = await _unitOfWork.Repository<Project>().GetAsync(projectId.Value, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new ProjectNotFoundException();
        }
    }

    /// <summary>
    /// Verifies the selected module exists in the system and is assigned to the selected project.
    /// </summary>
    public async Task ModuleShouldBelongToProjectAsync(int? projectId, int? moduleId, CancellationToken cancellationToken)
    {
        if (!moduleId.HasValue) return;

        var module = await _unitOfWork.Repository<Module>().GetAsync(moduleId.Value, cancellationToken);
        if (module == null || module.IsDeleted)
        {
            throw new ModuleNotFoundException();
        }

        if (projectId.HasValue)
        {
            var isAssigned = await _unitOfWork.Repository<ProjectModule>()
                .FindAsync(pm => pm.ProjectId == projectId.Value && pm.ModuleId == moduleId.Value && !pm.IsDeleted, cancellationToken);

            if (!isAssigned.Any())
            {
                throw new ModuleNotAssignedToProjectException();
            }
        }
    }
}
