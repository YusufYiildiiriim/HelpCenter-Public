using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;

public class UpdateRequestSubjectCommandHandler : IRequestHandler<UpdateRequestSubjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateRequestSubjectRules _rules;

    public UpdateRequestSubjectCommandHandler(IUnitOfWork unitOfWork, UpdateRequestSubjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateRequestSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _unitOfWork.Repository<RequestSubject>().FirstOrDefaultAsync(
            x => x.Id == request.Id, asTracking: true, cancellationToken: cancellationToken, x => x.Translations);
        UpdateRequestSubjectRules.SubjectShouldExist(subject);
        UpdateRequestSubjectRules.SubjectShouldNotBeLocked(subject!);

        await _rules.SubjectNameShouldBeUniqueAsync(request.Name, request.Id, cancellationToken);

        _unitOfWork.SetOriginalVersion(subject!, request.RowVersion);

        subject!.UpdateInfo(request.Name, request.Description, null, request.IsActive);
        if (request.Translations is not null)
        {
            subject.SourceLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale);
            var translations = request.Translations.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
            foreach (var obsolete in subject.Translations.Where(x => !translations.ContainsKey(x.Locale)).ToList())
                subject.Translations.Remove(obsolete);
            foreach (var pair in translations)
            {
                var translation = subject.Translations.FirstOrDefault(x => x.Locale == pair.Key);
                if (translation is null)
                {
                    translation = new RequestSubjectTranslation { Locale = pair.Key };
                    subject.Translations.Add(translation);
                }
                translation.Name = pair.Value.Name;
                translation.Description = pair.Value.Description;
            }
            if (subject.SourceLocale is not null && translations.TryGetValue(subject.SourceLocale, out var source))
            {
                subject.Name = source.Name;
                subject.Description = source.Description;
            }
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
