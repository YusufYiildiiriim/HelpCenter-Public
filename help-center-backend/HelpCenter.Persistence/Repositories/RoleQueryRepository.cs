using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Repositories;

public sealed class RoleQueryRepository : IRoleQueryRepository
{
    private readonly EfContext _efContext;

    public RoleQueryRepository(EfContext efContext)
    {
        _efContext = efContext;
    }

    public async Task<PaginatedResponse<RoleDto>> GetPaginatedAsync(
        string? search,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = pagination.PageNumber;
        var pageSize = pagination.PageSize;
        var unlimited = pagination.IsUnlimited;

        var normalizedSearch = search?.Trim().ToLower();

        IQueryable<Role> query = _efContext.Roles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(x =>
                x.Name.ToLower().Contains(normalizedSearch) ||
                (x.Description != null && x.Description.ToLower().Contains(normalizedSearch)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var projected = query.OrderBy(r => r.Id).Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            RowVersion = r.RowVersion,
            HasUsers = r.UserRoles.Any(),
            PermissionCount = r.RolePermissions.Count(p => p.Actions.Any(a => a.Action == "Read"))
        });

        var items = unlimited
            ? await projected.ToListAsync(cancellationToken)
            : await projected.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedResponse<RoleDto>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = unlimited ? (totalCount > 0 ? 1 : 0) : (int)Math.Ceiling(totalCount / (double)pageSize),
            CurrentPage = unlimited ? 1 : pageNumber,
            PageSize = unlimited ? totalCount : pageSize
        };
    }
}
