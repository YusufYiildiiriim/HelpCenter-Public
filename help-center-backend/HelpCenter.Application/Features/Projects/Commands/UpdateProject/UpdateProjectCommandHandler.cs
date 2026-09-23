using System.Linq.Expressions;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateProjectRules _rules;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateProjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId,
            asTracking: true,
            cancellationToken: cancellationToken,
            x => x.UserProjects, x => x.ProjectModules, x => x.Companies);

        UpdateProjectRules.ProjectShouldExist(project);

        await _rules.ProjectNameShouldBeUniqueAsync(request.Name, project!.Id, cancellationToken);
        UpdateProjectRules.ProjectShouldNotHaveActiveCompaniesWhenDeactivating(project!, request.IsActive);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(project!, request.RowVersion);

        project!.UpdateInfo(request.Name, request.Description, request.IsActive);

        if (request.UserIds != null)
        {
            project.SyncUsers(request.UserIds);
        }

        if (request.ModuleIds != null)
        {
            project.SyncModules(request.ModuleIds);
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicProjectsPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicFaqsPrefix);
        return true;
    }
}
