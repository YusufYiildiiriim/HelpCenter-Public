using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;

public record GetPublicFaqsQuery(
    int? ProjectId = null,
    int? ModuleId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 12) : IRequest<PaginatedResponse<PublicFaqDto>>;
