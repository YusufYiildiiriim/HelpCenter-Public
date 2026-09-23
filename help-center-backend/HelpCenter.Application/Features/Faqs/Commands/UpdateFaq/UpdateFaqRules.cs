using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

/// <summary>
/// Rules belonging only to the update FAQ slice.
/// </summary>
public class UpdateFaqRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFaqRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the FAQ record exists in the system.
    /// </summary>
    public static void FaqShouldExist(FAQ? faq)
    {
        if (faq == null || faq.IsDeleted)
        {
            throw new FaqNotFoundException();
        }
    }

    /// <summary>
    /// Verifies the FAQ title is unique.
    /// </summary>
    public async Task FaqTitleShouldBeUniqueAsync(string title, int currentFaqId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<FAQ>()
            .FindAsync(f => f.Title == title && f.Id != currentFaqId, cancellationToken);

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
