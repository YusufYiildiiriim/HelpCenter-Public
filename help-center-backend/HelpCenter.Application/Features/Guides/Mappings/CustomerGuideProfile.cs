using AutoMapper;
using HelpCenter.Application.Features.Guides.Queries;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides.Mappings;

public class CustomerGuideProfile : AutoMapper.Profile
{
    public CustomerGuideProfile()
    {
        CreateMap<Guide, PublicGuideDto>()
            .ForMember(dest => dest.PreviousGuidePublicId, opt => opt.Ignore());
        CreateMap<Document, PublicGuideDocumentDto>();
    }
}
