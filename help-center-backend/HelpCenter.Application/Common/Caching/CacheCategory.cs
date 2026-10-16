using HelpCenter.Application.Interfaces;
namespace HelpCenter.Application.Common.Caching;

public enum CacheCategory { PublicProjects, PublicFaqs, PublicModules, PublicGuides, Organization, Messages }

public static class CacheCategories
{
    public static string Prefix(this CacheCategory category) => category switch
    {
        CacheCategory.PublicProjects => CacheKeys.PublicProjectsPrefix,
        CacheCategory.PublicFaqs => CacheKeys.PublicFaqsPrefix,
        CacheCategory.PublicModules => CacheKeys.PublicModulesPrefix,
        CacheCategory.PublicGuides => CacheKeys.PublicGuidesPrefix,
        CacheCategory.Organization => CacheKeys.OrganizationInfoPrefix,
        CacheCategory.Messages => "messages",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };
    public static void Invalidate(this ICacheService cache, CacheCategory category) => cache.InvalidatePrefix(category.Prefix());
}

public interface IInvalidatesMessageCache { }
