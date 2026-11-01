using AutoMapper;
using HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetPublicRequestSubjects;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.RequestSubjects.Mappings;

public class RequestSubjectProfile : AutoMapper.Profile
{
    public RequestSubjectProfile()
    {
        CreateMap<RequestSubject, RequestSubjectDto>();
        CreateMap<RequestSubject, PublicRequestSubjectDto>();
        CreateMap<RequestSubject, CustomerRequestSubjectDto>();
        CreateMap<CreateRequestSubjectCommand, RequestSubject>()
            .ForMember(dest => dest.Translations, opt => opt.Ignore());
        CreateMap<UpdateRequestSubjectCommand, RequestSubject>()
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Translations, opt => opt.Ignore());
        CreateMap<RequestSubjectTranslation, HelpCenter.Application.Common.Models.LocalizedNameInput>();
    }
}
