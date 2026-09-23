using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;

public class GetRequestSubjectsQuery : IRequest<PaginatedResponse<RequestSubjectDto>>
{
    public bool OnlyActive { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
