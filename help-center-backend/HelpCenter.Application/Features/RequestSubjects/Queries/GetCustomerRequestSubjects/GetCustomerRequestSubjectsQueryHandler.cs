using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;

public class GetCustomerRequestSubjectsQueryHandler : IRequestHandler<GetCustomerRequestSubjectsQuery, List<CustomerRequestSubjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerRequestSubjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CustomerRequestSubjectDto>> Handle(GetCustomerRequestSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _unitOfWork.Repository<RequestSubject>().FindAsync(
            x => x.IsActive,
            cancellationToken, x => x.Translations
        );
        var requestedLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.Locale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        return subjects.Select(subject =>
        {
            var dto = _mapper.Map<CustomerRequestSubjectDto>(subject);
            var translation = subject.Translations.FirstOrDefault(x => x.Locale == requestedLocale)
                ?? (subject.SourceLocale is null ? null : subject.Translations.FirstOrDefault(x => x.Locale == subject.SourceLocale));
            dto.RequestedLocale = requestedLocale;
            if (translation is not null)
            {
                dto.Name = translation.Name;
                dto.ContentLocale = translation.Locale;
            }
            else
            {
                dto.ContentLocale = subject.SourceLocale;
            }
            return dto;
        }).ToList();
    }
}
