using AutoMapper;
using HelpCenter.Application.Features.Faqs.Queries;
using HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Faqs.Mappings;

public class CustomerFaqProfile : AutoMapper.Profile
{
    public CustomerFaqProfile()
    {
        CreateMap<FAQ, CustomerFaqDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : null));
    }
}
