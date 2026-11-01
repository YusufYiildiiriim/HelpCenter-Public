using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;

public class CreateRequestSubjectCommandHandler : IRequestHandler<CreateRequestSubjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateRequestSubjectRules _rules;

    public CreateRequestSubjectCommandHandler(IUnitOfWork unitOfWork, CreateRequestSubjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateRequestSubjectCommand request, CancellationToken cancellationToken)
    {
        await _rules.SubjectNameShouldBeUniqueAsync(request.Name, null, cancellationToken);

        var subject = RequestSubject.Create(request.Name, request.Description, null, request.IsActive);
        if (request.Translations.Count > 0)
        {
            subject.SourceLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale);
            foreach (var item in request.Translations)
                subject.Translations.Add(new RequestSubjectTranslation
                {
                    Locale = HelpCenter.Domain.Common.SupportedLocales.Normalize(item.Locale)!,
                    Name = item.Name,
                    Description = item.Description
                });
            var source = subject.Translations.Single(x => x.Locale == subject.SourceLocale);
            subject.Name = source.Name;
            subject.Description = source.Description;
        }

        await _unitOfWork.Repository<RequestSubject>().AddAsync(subject, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
