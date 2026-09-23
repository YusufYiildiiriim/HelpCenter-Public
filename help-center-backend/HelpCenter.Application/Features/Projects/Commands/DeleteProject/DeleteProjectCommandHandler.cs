using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProjectCommandHandler> _logger;
    private readonly ICacheService _cache;

    public DeleteProjectCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteProjectCommandHandler> logger,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Proje silme işlemi başlatıldı. Proje PublicId: {ProjectPublicId}", request.PublicId);

        var projects = await _unitOfWork.Repository<Project>()
            .FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.Companies);
        var project = projects.FirstOrDefault();
        DeleteProjectRules.ProjectShouldExist(project);

        DeleteProjectRules.ProjectShouldNotHaveActiveCompanies(project!);

        project!.MarkAsDeleted();
        await _unitOfWork.Repository<Project>().UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicProjectsPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicFaqsPrefix);

        _logger.LogInformation("Proje başarıyla silindi (Soft Delete). Proje: {ProjectName}", project.Name);

        return true;
    }
}
