using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

public class UpdateGuideCommandHandler : IRequestHandler<UpdateGuideCommand, bool>
{
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateGuideRules _rules;

    public UpdateGuideCommandHandler(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ICacheService cache,
        UpdateGuideRules rules)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateGuideCommand request, CancellationToken cancellationToken)
    {
        var guideRepository = _unitOfWork.Repository<Guide>();
        var entity = (await guideRepository.FindAsync(x => x.Id == request.Id, cancellationToken, x => x.Documents!, x => x.Translations)).FirstOrDefault();
        UpdateGuideRules.GuideShouldExist(entity);

        await _rules.GuideTitleShouldBeUniqueAsync(request.Title, entity!.Id, cancellationToken);

        _unitOfWork.SetOriginalVersion(entity!, request.RowVersion);

        entity!.UpdateInfo(
            request.Title,
            request.Description,
            request.Module,
            request.YoutubeUrl,
            request.PreviousGuideId,
            request.IsActive,
            request.IsPublic
        );

        if (HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale) is { } sourceLocale)
            entity.SourceLocale = sourceLocale;
        if (entity.SourceLocale is not null)
            SyncTranslations(entity, request.Translations ?? [], request.Translations is not null);

        var module = await HelpCenter.Application.Features.Guides.GuideModuleResolver.ResolveAsync(_unitOfWork, request.ModuleId, request.Module, cancellationToken);
        entity!.ModuleId = module.Id;
        entity.Module = module.Name;

        if (request.Files != null && request.Files.Count > 0)
        {
            entity.ClearDocuments();

            foreach (var file in request.Files)
            {
                string webPath = await _fileService.SaveFileAsync(file, "guides");
                entity.AddDocument(webPath, file.FileName);
            }
        }

        if (request.EnglishFiles?.Count > 0)
        {
            var englishTranslation = entity.Translations.FirstOrDefault(t => t.Locale == HelpCenter.Domain.Common.SupportedLocales.English);
            if (englishTranslation is not null)
            {
                var persistedTranslation = (await _unitOfWork.Repository<GuideTranslation>().FindAsync(
                    t => t.Id == englishTranslation.Id, cancellationToken, t => t.Documents)).FirstOrDefault();
                if (persistedTranslation is not null) englishTranslation = persistedTranslation;
            }
            if (englishTranslation is null)
            {
                englishTranslation = new GuideTranslation { Locale = HelpCenter.Domain.Common.SupportedLocales.English };
                entity.Translations.Add(englishTranslation);
            }
            englishTranslation.Documents.Clear();
            foreach (var file in request.EnglishFiles)
            {
                var path = await _fileService.SaveFileAsync(file, "guides/en-US");
                englishTranslation.Documents.Add(new GuideTranslationDocument { Path = path, FileName = file.FileName });
            }
        }
        await guideRepository.UpdateAsync(entity);
        var result = await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicGuides);
        _cache.Invalidate(CacheCategory.PublicModules);
        return result > 0;
    }

    private static void SyncTranslations(Guide guide, IReadOnlyCollection<HelpCenter.Application.Common.Models.LocalizedTextInput> inputs, bool removeMissing)
    {
        var values = inputs.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
        values[guide.SourceLocale!] = new() { Title = guide.Title, Description = guide.Description };
        if (removeMissing)
            foreach (var obsolete in guide.Translations.Where(t => !values.ContainsKey(t.Locale)).ToList())
                guide.Translations.Remove(obsolete);
        foreach (var pair in values)
        {
            var translation = guide.Translations.FirstOrDefault(t => t.Locale == pair.Key);
            if (translation is null)
            {
                translation = new GuideTranslation { Locale = pair.Key };
                guide.Translations.Add(translation);
            }
            translation.Title = pair.Value.Title;
            translation.Description = pair.Value.Description;
        }
    }
}
