using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetPublicModules;

public class GetPublicModulesQueryHandler : IRequestHandler<GetPublicModulesQuery, List<PublicModuleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetPublicModulesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public Task<List<PublicModuleDto>> Handle(GetPublicModulesQuery request, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.PublicModules(request.ProjectId),
            CacheKeys.PublicModulesPrefix,
            async ct =>
            {
                var projectId = request.ProjectId;
                List<int> projectModuleIds = new();
                List<string> guideModuleNames = new();

                if (projectId.HasValue && projectId.Value > 0)
                {
                    projectModuleIds = await _unitOfWork.Repository<ProjectModule>().SelectAsync(
                        pm => pm.ProjectId == projectId.Value,
                        pm => pm.ModuleId,
                        cancellationToken: ct);

                    guideModuleNames = await _unitOfWork.Repository<Guide>().SelectAsync(
                        g => g.IsActive && g.IsPublic && g.ProjectId == projectId.Value,
                        g => g.Module,
                        cancellationToken: ct,
                        distinct: true);
                }

                Expression<Func<Module, bool>> predicate = m =>
                    m.IsActive &&
                    (!projectId.HasValue || projectId.Value <= 0 ||
                     projectModuleIds.Contains(m.Id) ||
                     guideModuleNames.Contains(m.Name));

                var modules = await _unitOfWork.Repository<Module>().FindAsync(predicate, ct);
                return _mapper.Map<List<PublicModuleDto>>(modules);
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
