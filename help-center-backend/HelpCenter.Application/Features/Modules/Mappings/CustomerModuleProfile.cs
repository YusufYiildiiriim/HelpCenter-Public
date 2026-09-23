using AutoMapper;
using HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Mappings;

public class CustomerModuleProfile : AutoMapper.Profile
{
    public CustomerModuleProfile()
    {
        CreateMap<Module, CustomerModuleDto>();
    }
}
