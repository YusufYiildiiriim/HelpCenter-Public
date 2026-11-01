using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateModuleRules _rules;

    public UpdateModuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateModuleRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var modules = await _unitOfWork.Repository<Module>()
            .FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.ModuleExperts, x => x.Translations);

        var module = modules.FirstOrDefault();
        UpdateModuleRules.ModuleShouldExist(module);
        UpdateModuleRules.ModuleShouldNotBeLocked(module!);

        await _rules.ModuleNameShouldBeUniqueAsync(request.Name, module!.Id, cancellationToken);

        _unitOfWork.SetOriginalVersion(module!, request.RowVersion);

        module!.UpdateInfo(request.Name, request.Description, request.IsActive);
        if (request.Translations is not null)
        {
            module.SourceLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale);
            SyncTranslations(module, request.Translations);
        }

        if (request.ExpertUserIds != null)
        {
            if (request.ExpertUserIds.Any())
            {
                await _rules.UsersShouldExistAsync(request.ExpertUserIds, cancellationToken);
            }

            module.SyncExperts(request.ExpertUserIds);
        }

        await _unitOfWork.Repository<Module>().UpdateAsync(module!, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicModules);
        _cache.Invalidate(CacheCategory.PublicGuides);
        return true;
    }

    private static void SyncTranslations(Module module, IReadOnlyCollection<HelpCenter.Application.Common.Models.LocalizedNameInput> inputs)
    {
        var values = inputs.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
        foreach (var obsolete in module.Translations.Where(x => !values.ContainsKey(x.Locale)).ToList()) module.Translations.Remove(obsolete);
        foreach (var pair in values)
        {
            var translation = module.Translations.FirstOrDefault(x => x.Locale == pair.Key);
            if (translation is null) { translation = new ModuleTranslation { Locale = pair.Key }; module.Translations.Add(translation); }
            translation.Name = pair.Value.Name; translation.Description = pair.Value.Description;
        }
        if (module.SourceLocale is not null && values.TryGetValue(module.SourceLocale, out var source)) { module.Name = source.Name; module.Description = source.Description; }
    }
}
