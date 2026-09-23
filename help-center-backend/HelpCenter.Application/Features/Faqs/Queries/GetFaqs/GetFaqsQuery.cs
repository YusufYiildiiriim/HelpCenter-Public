using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetFaqs;

public class GetFaqsQuery : IRequest<PaginatedResponse<FaqDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public int? ProjectId { get; set; }
    public int? ModuleId { get; set; }
}
