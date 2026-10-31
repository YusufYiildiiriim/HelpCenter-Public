using AutoMapper;
using HelpCenter.Application.Features.Faqs.Commands.CreateFaq;
using HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;
using HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;
using HelpCenter.Application.Features.Faqs.Queries.GetFaqs;
using HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Faqs.Mappings;

public class FaqProfile : AutoMapper.Profile
{
    public FaqProfile()
    {
        CreateMap<FAQTranslation, HelpCenter.Application.Common.Models.LocalizedTextInput>();
        CreateMap<FAQ, FaqDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : null))
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ForMember(dest => dest.SourceLocale, opt => opt.MapFrom(src => src.SourceLocale));

        CreateMap<FAQ, PublicFaqDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : null));

        CreateMap<FAQ, CustomerFaqDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : null));

        CreateMap<CreateFaqCommand, FAQ>().ForMember(dest => dest.Translations, opt => opt.Ignore());
        CreateMap<UpdateFaqCommand, FAQ>().ForMember(dest => dest.RowVersion, opt => opt.Ignore()).ForMember(dest => dest.Translations, opt => opt.Ignore());
    }
}
