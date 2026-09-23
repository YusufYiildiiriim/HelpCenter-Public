using AutoMapper;
using HelpCenter.Application.Features.Users.Queries.GetUsers;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Users.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // NOTE: src.Account.X is used INSTEAD OF src.FirstName/LastName/Username/Email —
        // User.FirstName etc. are not real EF columns but C# properties that proxy to Account
        // (see User.cs: "get => Account?.FirstName"). ProjectTo (SQL projection) cannot translate
        // them to SQL, so we must go directly to the mapped field (Account.FirstName).
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Account.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Account.LastName))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)))
            .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => src.UserRoles));

        CreateMap<UserRole, UserRoleDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
    }
}
