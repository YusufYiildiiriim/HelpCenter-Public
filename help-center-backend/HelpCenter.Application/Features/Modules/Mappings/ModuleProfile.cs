using AutoMapper;
using HelpCenter.Application.Features.Modules.Commands.CreateModule;
using HelpCenter.Application.Features.Modules.Commands.UpdateModule;
using HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;
using HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;
using HelpCenter.Application.Features.Modules.Queries.GetModules;
using HelpCenter.Application.Features.Modules.Queries.GetPublicModules;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Mappings;

public class ModuleProfile : AutoMapper.Profile
{
    public ModuleProfile()
    {
        CreateMap<Module, ModuleDto>();
        CreateMap<Module, PublicModuleDto>();
        CreateMap<Module, CustomerModuleDto>();
        CreateMap<ModuleExpert, ModuleExpertDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName + " " + src.User.LastName : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));
        CreateMap<CreateModuleCommand, Module>();
        CreateMap<UpdateModuleCommand, Module>()
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}
