using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;

public class GetPublicFaqsQueryHandler : IRequestHandler<GetPublicFaqsQuery, PaginatedResponse<PublicFaqDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetPublicFaqsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public Task<PaginatedResponse<PublicFaqDto>> Handle(GetPublicFaqsQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();
        var pagination = new PaginationRequest(request.PageNumber, request.PageSize);

        // Same rationale as on the Guides side: requests with search are not cached.
        if (!string.IsNullOrWhiteSpace(search))
        {
            return FetchAsync(request, search, pagination, cancellationToken);
        }

        return _cache.GetOrCreateAsync(
            CacheKeys.PublicFaqs(request.ProjectId, request.ModuleId, pagination.PageNumber, pagination.PageSize),
            CacheKeys.PublicFaqsPrefix,
            ct => FetchAsync(request, search, pagination, ct),
            CacheKeys.PublicTtl,
            cancellationToken);
    }

    private async Task<PaginatedResponse<PublicFaqDto>> FetchAsync(
        GetPublicFaqsQuery request,
        string? search,
        PaginationRequest pagination,
        CancellationToken ct)
    {
        Expression<Func<FAQ, bool>> predicate = x =>
            x.IsActive && x.IsPublic &&
            (!request.ProjectId.HasValue || x.ProjectId == request.ProjectId.Value) &&
            (!request.ModuleId.HasValue || x.ModuleId == request.ModuleId.Value) &&
            (string.IsNullOrWhiteSpace(search) || x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search));

        var paginated = await _unitOfWork.Repository<FAQ>().GetPaginatedAsync(
            predicate,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken: ct,
            x => x.Project!, x => x.Module!);

        var dtoItems = _mapper.Map<List<PublicFaqDto>>(paginated.Items);

        return new PaginatedResponse<PublicFaqDto>
        {
            Items = dtoItems,
            TotalCount = paginated.TotalCount,
            TotalPages = paginated.TotalPages,
            CurrentPage = paginated.CurrentPage,
            PageSize = paginated.PageSize
        };
    }
}
