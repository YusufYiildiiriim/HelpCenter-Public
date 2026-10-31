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
        if (!string.IsNullOrWhiteSpace(search))
            return FetchAsync(request, search, pagination, cancellationToken);

        return _cache.GetOrCreateAsync(
            CacheKeys.PublicFaqs(request.ProjectId, request.ModuleId, pagination.PageNumber, pagination.PageSize, request.Locale),
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
            (string.IsNullOrWhiteSpace(search) ||
             x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search) ||
             x.Translations.Any(t => t.Title.ToLower().Contains(search) || t.Description.ToLower().Contains(search)));

        var paginated = await _unitOfWork.Repository<FAQ>().GetPaginatedAsync(
            predicate, pagination.PageNumber, pagination.PageSize, orderByKey: x => x.Title,
            cancellationToken: ct, x => x.Project!, x => x.Module!, x => x.Translations);

        var items = _mapper.Map<List<PublicFaqDto>>(paginated.Items);
        var company = request.ProjectId.HasValue
            ? await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(c => c.ProjectId == request.ProjectId.Value, cancellationToken: ct)
            : null;
        var companyLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(company?.DefaultLocale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;

        for (var i = 0; i < paginated.Items.Count; i++)
        {
            var faq = paginated.Items[i];
            var dto = items[i];
            dto.RequestedLocale = request.Locale;
            var translation = faq.Translations.FirstOrDefault(t => t.Locale == request.Locale)
                ?? faq.Translations.FirstOrDefault(t => t.Locale == companyLocale)
                ?? (faq.SourceLocale is null ? null : faq.Translations.FirstOrDefault(t => t.Locale == faq.SourceLocale));
            if (translation is not null)
            {
                dto.Title = translation.Title;
                dto.Description = translation.Description;
                dto.ContentLocale = translation.Locale;
            }
            else if (faq.SourceLocale is not null)
            {
                dto.ContentLocale = faq.SourceLocale;
            }
        }

        return new PaginatedResponse<PublicFaqDto>
        {
            Items = items,
            TotalCount = paginated.TotalCount,
            TotalPages = paginated.TotalPages,
            CurrentPage = paginated.CurrentPage,
            PageSize = paginated.PageSize
        };
    }
}