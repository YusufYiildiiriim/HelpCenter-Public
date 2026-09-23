using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 0) pageSize = 10;
        var unlimited = pageSize == 0;

        var list = source as IList<T> ?? source.ToList();
        var totalCount = list.Count;

        var items = unlimited
            ? list.ToList()
            : list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = unlimited ? (totalCount > 0 ? 1 : 0) : (int)Math.Ceiling(totalCount / (double)pageSize),
            CurrentPage = unlimited ? 1 : pageNumber,
            PageSize = unlimited ? totalCount : pageSize
        };
    }
}
