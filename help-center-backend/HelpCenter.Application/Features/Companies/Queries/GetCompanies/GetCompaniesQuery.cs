using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQuery : IRequest<PaginatedResponse<GetCompaniesResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 0;
    public string? Search { get; set; }
}
