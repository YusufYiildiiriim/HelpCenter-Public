using AutoMapper;
using HelpCenter.Application.Features.Companies.Commands;
using HelpCenter.Application.Features.Companies.Commands.CreateCompany;
using HelpCenter.Application.Features.Companies.Commands.DeleteCompany;
using HelpCenter.Application.Features.Companies.Commands.UpdateCompany;
using HelpCenter.Application.Features.Companies.Queries;
using HelpCenter.Application.Features.Companies.Queries.GetCompanies;

namespace HelpCenter.Application.Features.Companies.Mappings;

public class CompanyProfile : AutoMapper.Profile
{
    public CompanyProfile()
    {
        CreateMap<CreateCompanyCommand, Domain.Entities.Company>()
            .ForMember(dest => dest.ContactPersonUsername, opt => opt.MapFrom(src => src.ContactPersonUsername ?? string.Empty))
            .ForMember(dest => dest.CompanyModules, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore());
        CreateMap<UpdateCompanyCommand, Domain.Entities.Company>()
            .ForMember(dest => dest.ContactPersonUsername, opt => opt.MapFrom(src => src.ContactPersonUsername ?? string.Empty))
            .ForMember(dest => dest.CompanyModules, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
        CreateMap<DeleteCompanyCommand, Domain.Entities.Company>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => true));
        CreateMap<Domain.Entities.Company, GetCompaniesResponse>()
            .ForMember(dest => dest.ModuleIds, opt => opt.MapFrom(src => src.CompanyModules.Select(x => x.ModuleId).ToList()))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty));
    }
}
