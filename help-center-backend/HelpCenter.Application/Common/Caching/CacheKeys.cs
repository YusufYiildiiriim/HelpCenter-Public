namespace HelpCenter.Application.Common.Caching;

/// <summary>
/// Cache key prefixes for public/anonymous endpoints.
/// Command handlers invalidate these prefixes when the corresponding entity changes.
/// </summary>
public static class CacheKeys
{
    public const string PublicProjectsPrefix = "public:projects";
    public const string PublicFaqsPrefix = "public:faqs";
    public const string PublicModulesPrefix = "public:modules";
    public const string PublicGuidesPrefix = "public:guides";
    public const string OrganizationInfoPrefix = "org:info";

    public static readonly TimeSpan PublicTtl = TimeSpan.FromMinutes(5);

    public static string PublicProjects() => PublicProjectsPrefix;

    /// <summary>
    /// Only called for requests without search (Search empty) — see GetPublicFaqsQueryHandler.
    /// Page/pageSize are part of the key: the page/limit combination is finite and predictable,
    /// so there is no cache cardinality risk (the real risk is free-text search, which never enters the cache).
    /// </summary>
    public static string PublicFaqs(int? projectId, int? moduleId, int pageNumber, int pageSize) =>
        $"{PublicFaqsPrefix}:{projectId?.ToString() ?? "all"}:{moduleId?.ToString() ?? "all"}:{pageNumber}:{pageSize}";

    public static string PublicModules(int? projectId) => $"{PublicModulesPrefix}:{projectId?.ToString() ?? "all"}";

    /// <summary>Only called for requests without search (Search empty) — see GetPublicGuidesQueryHandler.</summary>
    public static string PublicGuides(int? projectId, string? module, int pageNumber, int pageSize) =>
        $"{PublicGuidesPrefix}:{projectId?.ToString() ?? "all"}:{module ?? "all"}:{pageNumber}:{pageSize}";
    public static string OrganizationInfoPublic() => $"{OrganizationInfoPrefix}:public";
    public static string OrganizationInfoAdmin() => $"{OrganizationInfoPrefix}:admin";
}
