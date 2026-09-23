using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;

/// <summary>
/// PageSize=0 (see <see cref="PaginationRequest"/>) returns unlimited/all results in one shot —
/// the portal intentionally calls it this way to fetch a module's ENTIRE guide chain
/// (GuideReaderModal forward/back navigation and the mind map). The default (grid) view is paged.
/// </summary>
public record GetPublicGuidesQuery(
    int? ProjectId = null,
    string? Module = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 12) : IRequest<PaginatedResponse<PublicGuideDto>>;
