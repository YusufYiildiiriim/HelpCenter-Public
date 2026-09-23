using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, List<RolePermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRolePermissionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RolePermissionDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.Repository<RolePermission>().FindAsync(
            x => x.RoleId == request.RoleId,
            cancellationToken,
            x => x.Actions);

        return permissions.Select(p => new RolePermissionDto
        {
            Id = p.Id,
            RoleId = p.RoleId,
            ResourceKey = p.ResourceKey,
            Actions = p.Actions.Select(a => a.Action).ToList(),
            AllowedFieldsJson = p.AllowedFieldsJson
        }).ToList();
    }
}
