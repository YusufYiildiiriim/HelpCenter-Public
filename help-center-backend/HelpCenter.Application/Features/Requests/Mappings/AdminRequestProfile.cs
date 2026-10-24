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
            // Project against the Account navigation explicitly. The User shortcut
            // properties are convenient for in-memory entities but are not a
            // reliable SQL projection target, which made assigned names blank on
            // the admin detail endpoint after an assignment.
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null
                ? $"{src.AssignedUser.Account.FirstName} {src.AssignedUser.Account.LastName}"
                : "Atanmadı"))
            // Keep this empty when no expert is assigned so the UI can fall back
            // to the handling agent name instead of treating "Uzman Yok" as a
            // selected expert.
            .ForMember(dest => dest.CurrentExpertName, opt => opt.MapFrom(src => src.CurrentExpert != null
                ? $"{src.CurrentExpert.Account.FirstName} {src.CurrentExpert.Account.LastName}"
                : string.Empty))
            .ForMember(dest => dest.RequestSubjectName, opt => opt.MapFrom(src => src.RequestSubject != null ? src.RequestSubject.Name : "Konu Yok"));
    }
}
