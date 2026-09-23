namespace HelpCenter.Application.Common.Models;

/// <summary>
/// PageNumber/PageSize normalization (pageNumber&lt;1 → 1, pageSize&lt;0 → 10, pageSize==0 →
/// unlimited) was duplicated verbatim across all features (see QueryableExtensions.ToPaginatedResponse,
/// GenericRepository.GetPaginatedProjectedAsync) — this type consolidates that duplication in one place. `Search`
/// is not included here: it is applied to different field(s) in each feature, so there is no shared contract.
/// </summary>
public readonly struct PaginationRequest
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool IsUnlimited => PageSize == 0;

    public PaginationRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 0 ? 10 : pageSize;
    }
}
