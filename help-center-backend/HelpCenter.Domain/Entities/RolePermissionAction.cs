using HelpCenter.Domain.Common;

namespace HelpCenter.Domain.Entities
{
    /// <summary>
    /// Join table holding which actions a RolePermission row has.
    /// Instead of bool columns (CanRead/CanCreate/...), action rows are kept, so
    /// adding a new action requires no migration — a row is simply added to this table.
    /// E.g.: for RolePermission { RoleId=2, ResourceKey="Projects" }, there may be
    /// RolePermissionAction rows { Action="Read" }, { Action="ManageMembers" }.
    /// </summary>
    public class RolePermissionAction : BaseEntity
    {
        public int RolePermissionId { get; set; }
        public string Action { get; set; } = string.Empty;

        public virtual RolePermission RolePermission { get; set; } = null!;
    }
}
