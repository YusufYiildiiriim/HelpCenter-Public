using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly CreateProjectRules _rules;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, CreateProjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        await _rules.ProjectNameShouldBeUniqueAsync(request.Name, null, cancellationToken);

        var project = Project.Create(request.Name, request.Description, request.IsActive);

        if (request.UserIds != null && request.UserIds.Any())
        {
            project.SyncUsers(request.UserIds);
        }

        if (request.ModuleIds != null && request.ModuleIds.Any())
        {
            project.SyncModules(request.ModuleIds);
        }

        await _unitOfWork.Repository<Project>().AddAsync(project, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicProjects);
        _cache.Invalidate(CacheCategory.PublicModules);
        _cache.Invalidate(CacheCategory.PublicGuides);
        _cache.Invalidate(CacheCategory.PublicFaqs);
        return true;
    }
}
