namespace HelpCenter.Application.Features.Menu.Queries.GetMenuItems;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? GroupTitle { get; set; }
    public int? ParentId { get; set; }
}
