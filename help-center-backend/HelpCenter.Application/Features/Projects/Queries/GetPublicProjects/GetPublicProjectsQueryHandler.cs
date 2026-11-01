using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;

public class GetPublicProjectsQueryHandler : IRequestHandler<GetPublicProjectsQuery, List<PublicProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetPublicProjectsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public Task<List<PublicProjectDto>> Handle(GetPublicProjectsQuery request, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.PublicProjects(HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale) ?? HelpCenter.Domain.Common.SupportedLocales.Default),
            CacheKeys.PublicProjectsPrefix,
            async ct =>
            {
                var projects = await _unitOfWork.Repository<Project>()
                    .FindAsync(x => x.IsActive, ct, x => x.Translations);

                var requestedLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale)
                    ?? HelpCenter.Domain.Common.SupportedLocales.Default;
                return projects.Select(p =>
                {
                    var translation = p.Translations.FirstOrDefault(x => x.Locale == requestedLocale)
                        ?? (p.SourceLocale is null ? null : p.Translations.FirstOrDefault(x => x.Locale == p.SourceLocale));
                    return new PublicProjectDto
                    {
                        Id = p.Id,
                        PublicId = p.PublicId,
                        Name = translation?.Name ?? p.Name,
                        Description = translation?.Description ?? p.Description,
                        RequestedLocale = requestedLocale,
                        ContentLocale = translation?.Locale ?? p.SourceLocale
                    };
                }).ToList();
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
