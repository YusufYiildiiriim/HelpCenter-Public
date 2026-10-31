using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

public class CreateGuideCommandHandler : IRequestHandler<CreateGuideCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;
    private readonly ICacheService _cache;
    private readonly CreateGuideRules _rules;

    public CreateGuideCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileService fileService,
        ICacheService cache,
        CreateGuideRules rules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateGuideCommand request, CancellationToken cancellationToken)
    {
        await _rules.GuideTitleShouldBeUniqueAsync(request.Title, null, cancellationToken);

        var entity = _mapper.Map<Guide>(request);
        entity.IsActive = request.IsActive;
        entity.IsPublic = request.IsPublic;
        entity.SourceLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale)
            ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        SyncTranslations(entity, request.Translations ?? []);

        var module = await HelpCenter.Application.Features.Guides.GuideModuleResolver.ResolveAsync(_unitOfWork, request.ModuleId, request.Module, cancellationToken);
        entity!.ModuleId = module.Id;
        entity.Module = module.Name;

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                string webPath = await _fileService.SaveFileAsync(file, "guides");
                entity.AddDocument(webPath, file.FileName);
            }
        }

        if (request.EnglishFiles?.Count > 0)
        {
            var englishTranslation = entity.Translations.First(t => t.Locale == HelpCenter.Domain.Common.SupportedLocales.English);
            foreach (var file in request.EnglishFiles)
            {
                var path = await _fileService.SaveFileAsync(file, "guides/en-US");
                englishTranslation.Documents.Add(new GuideTranslationDocument { Path = path, FileName = file.FileName });
            }
        }
        await _unitOfWork.Repository<Guide>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicGuides);
        _cache.Invalidate(CacheCategory.PublicModules);
        return true;
    }

    private static void SyncTranslations(Guide guide, IReadOnlyCollection<HelpCenter.Application.Common.Models.LocalizedTextInput> inputs)
    {
        var values = inputs.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
        values[guide.SourceLocale!] = new() { Title = guide.Title, Description = guide.Description };
        foreach (var pair in values)
            guide.Translations.Add(new GuideTranslation { Locale = pair.Key, Title = pair.Value.Title, Description = pair.Value.Description });
    }
}
