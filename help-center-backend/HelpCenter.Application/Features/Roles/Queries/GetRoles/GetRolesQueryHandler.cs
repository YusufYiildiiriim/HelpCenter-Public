using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PaginatedResponse<RoleDto>>
{
    private readonly IRoleQueryRepository _roleQueryRepository;

    public GetRolesQueryHandler(IRoleQueryRepository roleQueryRepository)
    {
        _roleQueryRepository = roleQueryRepository;
    }

    public Task<PaginatedResponse<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return _roleQueryRepository.GetPaginatedAsync(
            request.Search,
            new PaginationRequest(request.PageNumber, request.PageSize),
            cancellationToken);
    }
}
