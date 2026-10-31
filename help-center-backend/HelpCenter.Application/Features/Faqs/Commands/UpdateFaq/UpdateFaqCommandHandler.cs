using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommandHandler : IRequestHandler<UpdateFaqCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateFaqRules _rules;

    public UpdateFaqCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateFaqRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var matches = await _unitOfWork.Repository<FAQ>().FindAsync(x => x.Id == request.Id, cancellationToken, x => x.Translations);
        var faq = matches.FirstOrDefault();
        UpdateFaqRules.FaqShouldExist(faq);

        await _rules.FaqTitleShouldBeUniqueAsync(request.Title, request.Id, cancellationToken);
        await _rules.ProjectShouldExistAsync(request.ProjectId, cancellationToken);
        await _rules.ModuleShouldBelongToProjectAsync(request.ProjectId, request.ModuleId, cancellationToken);

        _unitOfWork.SetOriginalVersion(faq!, request.RowVersion);

        faq!.UpdateInfo(request.Title, request.Description, request.ProjectId, request.ModuleId, request.IsActive, request.IsPublic);
        if (HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale) is { } sourceLocale) faq.SourceLocale = sourceLocale;
        if (faq.SourceLocale is not null)
            SyncTranslations(faq, request.Translations ?? [], request.Translations is not null);

        await _unitOfWork.Repository<FAQ>().UpdateAsync(faq, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicFaqs);
        return true;
    }

    private static void SyncTranslations(FAQ faq, IReadOnlyCollection<HelpCenter.Application.Common.Models.LocalizedTextInput> inputs, bool removeMissing)
    {
        var values = inputs.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
        values[faq.SourceLocale!] = new() { Title = faq.Title, Description = faq.Description };
        if (removeMissing)
            foreach (var obsolete in faq.Translations.Where(t => !values.ContainsKey(t.Locale)).ToList())
                faq.Translations.Remove(obsolete);
        foreach (var pair in values)
        {
            var translation = faq.Translations.FirstOrDefault(t => t.Locale == pair.Key);
            if (translation is null)
            {
                translation = new FAQTranslation { Locale = pair.Key };
                faq.Translations.Add(translation);
            }
            translation.Title = pair.Value.Title;
            translation.Description = pair.Value.Description;
        }
    }
}
