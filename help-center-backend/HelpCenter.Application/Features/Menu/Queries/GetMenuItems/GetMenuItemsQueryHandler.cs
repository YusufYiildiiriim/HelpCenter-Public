using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Menu.Queries.GetMenuItems;

public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, List<MenuItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public GetMenuItemsQueryHandler(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<List<MenuItemDto>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        if (userId <= 0)
        {
            return new List<MenuItemDto>();
        }

        // Previously the entire role/permission graph was fetched via an Include on the
        // "Role.RolePermissions.Actions" string path and filtered in C#. Now only the needed
        // "Read"-authorized ResourceKeys are projected directly in SQL.
        var resourceKeyGroups = await _unitOfWork.Repository<UserRole>().SelectAsync(
            x => x.UserId == userId && x.Role != null && x.Role.IsActive,
            x => x.Role!.RolePermissions
                .Where(rp => rp.Actions.Any(a => a.Action == "Read"))
                .Select(rp => rp.ResourceKey),
            cancellationToken: cancellationToken);

        var allowedResourceKeys = resourceKeyGroups.SelectMany(x => x).ToHashSet();

        var allItems = await _unitOfWork.Repository<MenuItem>()
            .FindAsync(x => x.IsActive, cancellationToken);

        var definedResourceKeys = AppResourceDefinitions.All.Select(x => x.Key).ToHashSet();
        var allowedItems = allItems
            .Where(m => definedResourceKeys.Contains(m.ResourceKey) && allowedResourceKeys.Contains(m.ResourceKey))
            .OrderBy(m => m.Order)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Label = m.Label,
                Icon = m.Icon,
                Route = m.Route,
                ResourceKey = m.ResourceKey,
                Order = m.Order,
                GroupTitle = m.GroupTitle,
                ParentId = m.ParentId
            })
            .ToList();

        return allowedItems;
    }
}
