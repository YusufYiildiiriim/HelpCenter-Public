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

        // Since search is free text, only search-less requests are cached (the grid's default
        // pages, module chain fetches) to avoid blowing up the cache cardinality with random
        // terms; searches always go straight to the DB.
        if (!string.IsNullOrWhiteSpace(search))
        {
            return FetchAsync(request, search, pagination, cancellationToken);
        }

        return _cache.GetOrCreateAsync(
            CacheKeys.PublicGuides(request.ProjectId, request.Module, pagination.PageNumber, pagination.PageSize),
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
        List<string> projectModuleNames = new();

        if (projectId.HasValue && projectId.Value > 0)
        {
            var projectModuleIds = await _unitOfWork.Repository<ProjectModule>().SelectAsync(
                pm => pm.ProjectId == projectId.Value,
                pm => pm.ModuleId,
                cancellationToken: ct);

            projectModuleNames = await _unitOfWork.Repository<Module>().SelectAsync(
                m => projectModuleIds.Contains(m.Id),
                m => m.Name,
                cancellationToken: ct);
        }

        Expression<Func<Guide, bool>> predicate = g =>
            g.IsActive && g.IsPublic &&
            (!projectId.HasValue || projectId.Value <= 0 ||
             g.ProjectId == projectId.Value ||
             (!g.ProjectId.HasValue && projectModuleNames.Contains(g.Module))) &&
            (string.IsNullOrWhiteSpace(request.Module) || g.Module == request.Module) &&
            (string.IsNullOrWhiteSpace(search) || g.Title.ToLower().Contains(search) || g.Description.ToLower().Contains(search));

        var paginated = await _unitOfWork.Repository<Guide>().GetPaginatedAsync(
            predicate,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken: ct,
            g => g.Documents!);

        var guides = paginated.Items;
        var dtos = _mapper.Map<List<PublicGuideDto>>(guides);

        // The previous step's publicId is resolved via a separate, unpaged ID lookup so that it
        // resolves correctly even when it falls outside the page boundary (see GuideReaderModal
        // forward/back navigation — the chain can cross page boundaries; this lookup always
        // scans the whole table).
        var previousGuideIds = guides.Where(g => g.PreviousGuideId.HasValue)
            .Select(g => g.PreviousGuideId!.Value).Distinct().ToList();
        var previousGuidePublicIds = previousGuideIds.Count > 0
            ? (await _unitOfWork.Repository<Guide>().SelectAsync(
                g => previousGuideIds.Contains(g.Id),
                g => new { g.Id, g.PublicId },
                cancellationToken: ct))
              .ToDictionary(x => x.Id, x => x.PublicId)
            : new Dictionary<int, Guid>();

        for (int i = 0; i < guides.Count; i++)
        {
            dtos[i].PublicId = guides[i].PublicId;
            dtos[i].PreviousGuidePublicId = guides[i].PreviousGuideId is int prevId
                && previousGuidePublicIds.TryGetValue(prevId, out var prevPublicId)
                ? prevPublicId
                : null;
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
