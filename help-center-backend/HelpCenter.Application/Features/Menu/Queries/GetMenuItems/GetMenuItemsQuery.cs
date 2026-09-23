using MediatR;

namespace HelpCenter.Application.Features.Menu.Queries.GetMenuItems;

public record GetMenuItemsQuery() : IRequest<List<MenuItemDto>>;
