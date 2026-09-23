using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;

namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Rather than adding a general projection method to `IGenericRepository&lt;Role&gt;` for the roles list
/// (see the "General Repository Assortment" thread in Threads.md), this is a narrow read contract
/// specific to this feature only. The `RoleDto` shape (including the HasUsers/PermissionCount
/// aggregations) and the search/pagination logic are defined here — implemented in Persistence.
/// </summary>
public interface IRoleQueryRepository
{
    Task<PaginatedResponse<RoleDto>> GetPaginatedAsync(
        string? search,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
