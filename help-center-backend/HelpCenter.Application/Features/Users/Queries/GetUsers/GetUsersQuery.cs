using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<PaginatedResponse<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 0;
    public string? Search { get; set; }
}
