using HelpCenter.Domain.Common;


namespace HelpCenter.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        public int RoleId { get; set; }
        public string ResourceKey { get; set; } = string.Empty;

        /// <summary>
        /// JSON array of allowed fields for dynamic field/widget-based authorization.
        /// E.g.: ["totalRequests", "activeRequests", "totalMessages"]
        /// If null or empty, all fields across the resource are allowed.
        /// </summary>
        public string? AllowedFieldsJson { get; set; }

        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<RolePermissionAction> Actions { get; set; } = new HashSet<RolePermissionAction>();
    }
}
