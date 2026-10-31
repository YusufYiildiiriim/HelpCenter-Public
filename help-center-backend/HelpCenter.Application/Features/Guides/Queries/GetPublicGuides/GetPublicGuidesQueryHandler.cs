using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;

public class GetPublicGuidesQueryHandler : IRequestHandler<GetPublicGuidesQuery, PaginatedResponse<PublicGuideDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetPublicGuidesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public Task<PaginatedResponse<PublicGuideDto>> Handle(GetPublicGuidesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();
        var pagination = new PaginationRequest(request.PageNumber, request.PageSize);
        if (!string.IsNullOrWhiteSpace(search))
            return FetchAsync(request, search, pagination, cancellationToken);

        return _cache.GetOrCreateAsync(
            CacheKeys.PublicGuides(request.ProjectId, request.Module, pagination.PageNumber, pagination.PageSize, request.Locale),
            CacheKeys.PublicGuidesPrefix,
            ct => FetchAsync(request, search, pagination, ct),
            CacheKeys.PublicTtl,
            cancellationToken);
    }

    private async Task<PaginatedResponse<PublicGuideDto>> FetchAsync(
        GetPublicGuidesQuery request,
        string? search,
        PaginationRequest pagination,
        CancellationToken ct)
    {
        var projectId = request.ProjectId;
        List<Guid> projectModuleIds = new();
        List<string> projectModuleNames = new();

        if (projectId.HasValue && projectId.Value != Guid.Empty)
        {
            projectModuleIds = await _unitOfWork.Repository<ProjectModule>().SelectAsync(
                pm => pm.ProjectId == projectId.Value, pm => pm.ModuleId, cancellationToken: ct);
            projectModuleNames = await _unitOfWork.Repository<Module>().SelectAsync(
                m => projectModuleIds.Contains(m.Id), m => m.Name, cancellationToken: ct);
        }

        Expression<Func<Guide, bool>> predicate = g =>
            g.IsActive && g.IsPublic &&
            (!projectId.HasValue || projectId.Value == Guid.Empty ||
             g.ProjectId == projectId.Value ||
             (!g.ProjectId.HasValue && (g.ModuleId.HasValue ? projectModuleIds.Contains(g.ModuleId.Value) : projectModuleNames.Contains(g.Module)))) &&
            (string.IsNullOrWhiteSpace(request.Module) || (g.ModuleEntity != null ? g.ModuleEntity.Name : g.Module) == request.Module) &&
            (string.IsNullOrWhiteSpace(search) ||
             g.Title.ToLower().Contains(search) || g.Description.ToLower().Contains(search) ||
             g.Translations.Any(t => t.Title.ToLower().Contains(search) || t.Description.ToLower().Contains(search)));

        var paginated = await _unitOfWork.Repository<Guide>().GetPaginatedAsync(
            predicate, pagination.PageNumber, pagination.PageSize, orderByKey: x => x.Title,
            cancellationToken: ct, g => g.Documents!, g => g.ModuleEntity!, g => g.Translations);

        var guides = paginated.Items;
        var dtos = _mapper.Map<List<PublicGuideDto>>(guides);
        var company = projectId.HasValue
            ? await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(c => c.ProjectId == projectId.Value, cancellationToken: ct)
            : null;
        var companyLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(company?.DefaultLocale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        var guideIds = guides.Select(g => g.Id).ToList();
        var localizedGuideDocs = guideIds.Count == 0
            ? new List<GuideTranslation>()
            : (await _unitOfWork.Repository<GuideTranslation>().FindAsync(
                t => guideIds.Contains(t.GuideId), ct, t => t.Documents)).ToList();

        for (var i = 0; i < guides.Count; i++)
        {
            var guide = guides[i];
            var dto = dtos[i];
            dto.RequestedLocale = request.Locale;
            var translation = guide.Translations.FirstOrDefault(t => t.Locale == request.Locale)
                ?? guide.Translations.FirstOrDefault(t => t.Locale == companyLocale)
                ?? (guide.SourceLocale is null ? null : guide.Translations.FirstOrDefault(t => t.Locale == guide.SourceLocale));
            if (translation is not null)
            {
                dto.Title = translation.Title;
                dto.Description = translation.Description;
                dto.ContentLocale = translation.Locale;
                var localized = localizedGuideDocs.FirstOrDefault(t => t.Id == translation.Id);
                var translationDocuments = localized?.Documents
                    .Select(d => new PublicGuideDocumentDto { Path = d.Path, FileName = d.FileName })
                    .ToList();
                if (translation.Locale == request.Locale && translationDocuments is { Count: > 0 })
                {
                    dto.Documents = translationDocuments;
                }
                else if (translation.Locale == request.Locale
                    && HelpCenter.Domain.Common.SupportedLocales.Normalize(guide.SourceLocale) == request.Locale)
                {
                    dto.Documents = guide.Documents?
                        .Select(document => new PublicGuideDocumentDto { Path = document.Path, FileName = document.FileName })
                        .ToList() ?? new List<PublicGuideDocumentDto>();
                }
                else
                {
                    dto.Documents = new();
                }
            }
            else
            {
                dto.ContentLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(guide.SourceLocale);
                if (dto.ContentLocale != request.Locale) dto.Documents = new();
            }
        }

        var previousGuideIds = guides.Where(g => g.PreviousGuideId.HasValue)
            .Select(g => g.PreviousGuideId!.Value).Distinct().ToList();
        var previousGuidePublicIds = previousGuideIds.Count > 0
            ? (await _unitOfWork.Repository<Guide>().SelectAsync(
                g => previousGuideIds.Contains(g.Id), g => new { g.Id, g.PublicId }, cancellationToken: ct))
              .ToDictionary(x => x.Id, x => x.PublicId)
            : new Dictionary<Guid, Guid>();

        for (int i = 0; i < guides.Count; i++)
        {
            dtos[i].PublicId = guides[i].PublicId;
            dtos[i].PreviousGuidePublicId = guides[i].PreviousGuideId is Guid prevId
                && previousGuidePublicIds.TryGetValue(prevId, out var prevPublicId) ? prevPublicId : null;
        }

        return new PaginatedResponse<PublicGuideDto>
        {
            Items = dtos,
            TotalCount = paginated.TotalCount,
            TotalPages = paginated.TotalPages,
            CurrentPage = paginated.CurrentPage,
            PageSize = paginated.PageSize
        };
    }
}
