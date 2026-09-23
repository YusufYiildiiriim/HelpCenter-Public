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
        CreateMap<Guide, GuideDto>()
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0));
        CreateMap<Document, GuideDocumentDto>();
        CreateMap<Guide, PublicGuideDto>();
        CreateMap<Document, PublicGuideDocumentDto>();
        CreateMap<CreateGuideCommand, Guide>();
        CreateMap<UpdateGuideCommand, Guide>()
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}
