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
            CacheKeys.PublicModules(request.ProjectId, HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale) ?? HelpCenter.Domain.Common.SupportedLocales.Default),
            CacheKeys.PublicModulesPrefix,
            async ct =>
            {
                var projectId = request.ProjectId;
                List<Guid> projectModuleIds = new();
                List<Guid?> guideModuleIds = new();
                List<string> guideModuleNames = new();

                if (projectId.HasValue && projectId.Value != Guid.Empty)
                {
                    projectModuleIds = await _unitOfWork.Repository<ProjectModule>().SelectAsync(
                        pm => pm.ProjectId == projectId.Value,
                        pm => pm.ModuleId,
                        cancellationToken: ct);

                    guideModuleIds = await _unitOfWork.Repository<Guide>().SelectAsync(
                        g => g.IsActive && g.IsPublic && g.ProjectId == projectId.Value && g.ModuleId.HasValue,
                        g => g.ModuleId, cancellationToken: ct, distinct: true);
                    guideModuleNames = await _unitOfWork.Repository<Guide>().SelectAsync(
                        g => g.IsActive && g.IsPublic && g.ProjectId == projectId.Value && !g.ModuleId.HasValue,
                        g => g.Module,
                        cancellationToken: ct,
                        distinct: true);
                }

                Expression<Func<Module, bool>> predicate = m =>
                    m.IsActive &&
                    (!projectId.HasValue || projectId.Value == Guid.Empty ||
                     projectModuleIds.Contains(m.Id) ||
                     guideModuleIds.Contains(m.Id) || guideModuleNames.Contains(m.Name));

                var modules = await _unitOfWork.Repository<Module>().FindAsync(predicate, ct, x => x.Translations);
                var requestedLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale)
                    ?? HelpCenter.Domain.Common.SupportedLocales.Default;
                return modules.Select(module =>
                {
                    var translation = module.Translations.FirstOrDefault(x => x.Locale == requestedLocale)
                        ?? (module.SourceLocale is null ? null : module.Translations.FirstOrDefault(x => x.Locale == module.SourceLocale));
                    return new PublicModuleDto
                    {
                        Id = module.Id,
                        Name = translation?.Name ?? module.Name,
                        Description = translation?.Description ?? module.Description,
                        RequestedLocale = requestedLocale,
                        ContentLocale = translation?.Locale ?? module.SourceLocale
                    };
                }).ToList();
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
