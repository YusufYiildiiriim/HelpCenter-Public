using AutoMapper;
using HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Mappings;

public class CustomerRequestProfile : Profile
{
    public CustomerRequestProfile()
    {
        CreateMap<CustomerRequest, CustomerRequestDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.RequestSubjectName, opt => opt.MapFrom(src => src.RequestSubject != null ? src.RequestSubject.Name : "Konu Yok"));
    }
}
