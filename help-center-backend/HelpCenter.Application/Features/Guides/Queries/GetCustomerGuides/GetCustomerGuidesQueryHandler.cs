using AutoMapper;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Common;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;

public class GetCustomerGuidesQueryHandler : IRequestHandler<GetCustomerGuidesQuery, List<PublicGuideDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerGuidesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PublicGuideDto>> Handle(GetCustomerGuidesQuery request, CancellationToken cancellationToken)
    {
        var requestedLocale = SupportedLocales.Normalize(request.Locale) ?? SupportedLocales.Default;
        var company = await _unitOfWork.Repository<Company>().FirstOrDefaultAsync(
            company => company.Id == request.CompanyId, cancellationToken: cancellationToken);
        var companyLocale = SupportedLocales.Normalize(company?.DefaultLocale) ?? SupportedLocales.Default;

        var guides = (await _unitOfWork.Repository<Guide>().FindAsync(
            g => g.IsActive,
            cancellationToken,
            g => g.Documents!, g => g.ModuleEntity!, g => g.Translations
        )).ToList();

        var dtos = _mapper.Map<List<PublicGuideDto>>(guides);

        var guideIds = guides.Select(guide => guide.Id).ToList();
        var localizedTranslations = guideIds.Count == 0
            ? new List<GuideTranslation>()
            : (await _unitOfWork.Repository<GuideTranslation>().FindAsync(
                translation => guideIds.Contains(translation.GuideId), cancellationToken,
                translation => translation.Documents)).ToList();

        for (var i = 0; i < guides.Count; i++)
        {
            var guide = guides[i];
            var dto = dtos[i];
            dto.RequestedLocale = requestedLocale;

            var translation = guide.Translations.FirstOrDefault(item => item.Locale == requestedLocale)
                ?? guide.Translations.FirstOrDefault(item => item.Locale == companyLocale)
                ?? (SupportedLocales.Normalize(guide.SourceLocale) is { } sourceLocale
                    ? guide.Translations.FirstOrDefault(item => item.Locale == sourceLocale)
                    : null);

            if (translation is not null)
            {
                dto.Title = translation.Title;
                dto.Description = translation.Description;
                dto.ContentLocale = translation.Locale;

                var localized = localizedTranslations.FirstOrDefault(item => item.Id == translation.Id);
                var localizedDocuments = localized?.Documents
                    .Select(document => new PublicGuideDocumentDto { Path = document.Path, FileName = document.FileName })
                    .ToList();
                if (localizedDocuments is { Count: > 0 })
                {
                    dto.Documents = localizedDocuments;
                }
                else if (SupportedLocales.Normalize(guide.SourceLocale) != translation.Locale)
                {
                    dto.Documents = new List<PublicGuideDocumentDto>();
                }
            }
            else
            {
                dto.ContentLocale = SupportedLocales.Normalize(guide.SourceLocale);
            }
        }

        var previousGuideIds = guides.Where(g => g.PreviousGuideId.HasValue)
            .Select(g => g.PreviousGuideId!.Value).Distinct().ToList();
        var previousGuidePublicIds = previousGuideIds.Count > 0
            ? (await _unitOfWork.Repository<Guide>().SelectAsync(
                g => previousGuideIds.Contains(g.Id),
                g => new { g.Id, g.PublicId },
                cancellationToken: cancellationToken))
              .ToDictionary(x => x.Id, x => x.PublicId)
            : new Dictionary<Guid, Guid>();

        for (int i = 0; i < guides.Count; i++)
        {
            dtos[i].PublicId = guides[i].PublicId;
            dtos[i].PreviousGuidePublicId = guides[i].PreviousGuideId is Guid prevId
                && previousGuidePublicIds.TryGetValue(prevId, out var prevPublicId)
                ? prevPublicId
                : null;
        }

        return dtos;
    }
}
