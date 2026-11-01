using System.Linq.Expressions;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateProjectRules _rules;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateProjectRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId,
            asTracking: true,
            cancellationToken: cancellationToken,
            x => x.UserProjects, x => x.ProjectModules, x => x.Companies, x => x.Translations);

        UpdateProjectRules.ProjectShouldExist(project);

        await _rules.ProjectNameShouldBeUniqueAsync(request.Name, project!.Id, cancellationToken);
        UpdateProjectRules.ProjectShouldNotHaveActiveCompaniesWhenDeactivating(project!, request.IsActive);

        _unitOfWork.SetOriginalVersion(project!, request.RowVersion);

        project!.UpdateInfo(request.Name, request.Description, request.IsActive);
        if (request.Translations is not null)
        {
            project.SourceLocale = HelpCenter.Domain.Common.SupportedLocales.Normalize(request.SourceLocale);
            SyncTranslations(project, request.Translations);
        }

        if (request.UserIds != null)
        {
            project.SyncUsers(request.UserIds);
        }

        if (request.ModuleIds != null)
        {
            project.SyncModules(request.ModuleIds);
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Invalidate(CacheCategory.PublicProjects);
        _cache.Invalidate(CacheCategory.PublicModules);
        _cache.Invalidate(CacheCategory.PublicGuides);
        _cache.Invalidate(CacheCategory.PublicFaqs);
        return true;
    }

    private static void SyncTranslations(Project project, IReadOnlyCollection<HelpCenter.Application.Common.Models.LocalizedNameInput> inputs)
    {
        var values = inputs.ToDictionary(x => HelpCenter.Domain.Common.SupportedLocales.Normalize(x.Locale)!, StringComparer.Ordinal);
        foreach (var obsolete in project.Translations.Where(x => !values.ContainsKey(x.Locale)).ToList()) project.Translations.Remove(obsolete);
        foreach (var pair in values)
        {
            var translation = project.Translations.FirstOrDefault(x => x.Locale == pair.Key);
            if (translation is null) { translation = new ProjectTranslation { Locale = pair.Key }; project.Translations.Add(translation); }
            translation.Name = pair.Value.Name; translation.Description = pair.Value.Description;
        }
        if (project.SourceLocale is not null && values.TryGetValue(project.SourceLocale, out var source)) { project.Name = source.Name; project.Description = source.Description; }
    }
}
