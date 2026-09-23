using AutoMapper;
using HelpCenter.Application.Features.Roles.Queries;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Features.Roles.Queries.GetUserPermissions;

namespace HelpCenter.Application.Features.Roles.Mappings;

public class RoleProfile : AutoMapper.Profile
{
    public RoleProfile()
    {
        CreateMap<Domain.Entities.RolePermission, ModulePermissionDto>()
            .ForMember(d => d.ResourceKey, o => o.MapFrom(s => s.ResourceKey));

        CreateMap<Domain.Entities.Role, RoleDto>()
            .ForMember(d => d.HasUsers, o => o.MapFrom(s => s.UserRoles != null && s.UserRoles.Any()))
            .ForMember(d => d.PermissionCount, o => o.MapFrom(s => s.RolePermissions != null ? s.RolePermissions.Count(p => p.Actions.Any(a => a.Action == "Read")) : 0));
    }
}
