using AutoMapper;
using HelpCenter.Application.Features.Guides.Commands.CreateGuide;
using HelpCenter.Application.Features.Guides.Commands.UpdateGuide;
using HelpCenter.Application.Features.Guides.Queries.GetGuides;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides.Mappings;

public class GuideProfile : AutoMapper.Profile
{
    public GuideProfile()
    {
        CreateMap<GuideTranslation, HelpCenter.Application.Common.Models.LocalizedTextInput>();
        CreateMap<Guide, GuideDto>()
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.ModuleEntity != null ? src.ModuleEntity.Name : src.Module))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0))
            .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.Translations))
            .ForMember(dest => dest.SourceLocale, opt => opt.MapFrom(src => src.SourceLocale));
        CreateMap<Document, GuideDocumentDto>();
        CreateMap<Guide, PublicGuideDto>()
            .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.ModuleEntity != null ? src.ModuleEntity.Name : src.Module));
        CreateMap<Document, PublicGuideDocumentDto>();
        CreateMap<CreateGuideCommand, Guide>().ForMember(dest => dest.Translations, opt => opt.Ignore());
        CreateMap<UpdateGuideCommand, Guide>()
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore());
    }
}
