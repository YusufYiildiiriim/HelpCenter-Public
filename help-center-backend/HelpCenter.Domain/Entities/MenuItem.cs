using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities
{
    public class MenuItem : BaseEntity
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string ResourceKey { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int Order { get; set; }
        public string? GroupTitle { get; set; }

        public virtual MenuItem? Parent { get; set; }
        public virtual ICollection<MenuItem> Children { get; set; } = new HashSet<MenuItem>();

        public static MenuItem Create(string label, string icon, string route, string resourceKey, int order, string? groupTitle = null, int? parentId = null)
        {
            return new MenuItem
            {
                Label = label,
                Icon = icon,
                Route = route,
                ResourceKey = resourceKey,
                Order = order,
                GroupTitle = groupTitle,
                ParentId = parentId,
                IsActive = true
            };
        }
    }
}
