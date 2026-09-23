using AutoMapper;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Requests.Mappings;

public class AdminRequestProfile : AutoMapper.Profile
{
    public AdminRequestProfile()
    {
        CreateMap<CustomerRequest, AdminRequestDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Customer.Company.Name))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : ""))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents.Count))
            .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Conversation != null ? src.Conversation.Messages.Count : 0))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? $"{src.AssignedUser.FirstName} {src.AssignedUser.LastName}" : "Atanmadı"))
            .ForMember(dest => dest.CurrentExpertName, opt => opt.MapFrom(src => src.CurrentExpert != null ? $"{src.CurrentExpert.FirstName} {src.CurrentExpert.LastName}" : "Uzman Yok"))
            .ForMember(dest => dest.RequestSubjectName, opt => opt.MapFrom(src => src.RequestSubject != null ? src.RequestSubject.Name : "Konu Yok"));
    }
}
