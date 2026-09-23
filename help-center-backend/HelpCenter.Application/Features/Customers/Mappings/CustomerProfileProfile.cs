using AutoMapper;
using HelpCenter.Application.Features.Auth.Queries.CustomerLogin;
using HelpCenter.Application.Features.Customers.Queries;
using HelpCenter.Application.Features.Customers.Queries.GetCustomers;
using HelpCenter.Application.Features.Customers.Queries.GetProfile;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Customers.Mappings;

public class CustomerProfileProfile : AutoMapper.Profile
{
    public CustomerProfileProfile()
    {
        CreateMap<HelpCenter.Domain.Entities.Customer, CustomerProfileDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Account.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Account.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber ?? string.Empty));

        CreateMap<Customer, CustomerDetailDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Account.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Account.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Account.Email))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account.Username))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Account.PhoneNumber ?? string.Empty))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Customer, CustomerLoginResponse>()
            .ForMember(dest => dest.IsPasswordChangeRequired, opt => opt.MapFrom(src => src.Account.IsPasswordChangeRequired))
            .ForMember(dest => dest.CompanyPublicId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerPublicId, opt => opt.Ignore())
            .ForMember(dest => dest.Success, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Message, opt => opt.Ignore());
    }
}
