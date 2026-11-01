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
        var companyLocale = (await ResolveCompanyAsync(request, cancellationToken))?.DefaultLocale;
        var fallbackLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(companyLocale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        return subjects.Select(subject =>
        {
            var dto = _mapper.Map<CustomerRequestSubjectDto>(subject);
            var translation = subject.Translations.FirstOrDefault(x => x.Locale == requestedLocale)
                ?? subject.Translations.FirstOrDefault(x => x.Locale == fallbackLocale)
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

    private async Task<Company?> ResolveCompanyAsync(GetCustomerRequestSubjectsQuery request, CancellationToken cancellationToken)
    {
        if (request.CompanyId != Guid.Empty)
            return await _unitOfWork.Repository<Company>().GetAsync(request.CompanyId, cancellationToken);
        if (request.CustomerId == Guid.Empty) return null;
        var customer = (await _unitOfWork.Repository<Customer>().FindAsync(
            x => x.Id == request.CustomerId, cancellationToken, x => x.Company)).FirstOrDefault();
        return customer?.Company;
    }
}
