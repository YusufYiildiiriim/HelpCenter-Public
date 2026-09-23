namespace HelpCenter.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new HashSet<RolePermission>();

        public static Role Create(string name, string? description)
        {
            return new Role
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        public void UpdateInfo(string name, string? description, bool isActive)
        {
            Name = name;
            Description = description;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Syncs permissions based on the action-list model (source of truth).
        /// Each string in the Actions list is managed as a RolePermissionAction row.
        /// The standard CRUD bool fields are derived from the action list for backward compat.
        /// Resources with an empty action list are soft-deleted entirely.
        /// </summary>
        public void SyncPermissions(IEnumerable<(string ResourceKey, IReadOnlyList<string> Actions, string? AllowedFieldsJson)> permissions)
        {
            var permList = permissions.ToList();
            var incomingKeys = permList.Where(p => p.Actions.Count > 0).Select(p => p.ResourceKey).ToHashSet();

            // Soft-delete resources that are absent from the incoming request
            var toRemove = RolePermissions.Where(x => !incomingKeys.Contains(x.ResourceKey) && !x.IsDeleted).ToList();
            foreach (var rp in toRemove)
            {
                rp.MarkAsDeleted();
                foreach (var a in rp.Actions.Where(a => !a.IsDeleted))
                    a.MarkAsDeleted();
            }

            foreach (var p in permList)
            {
                if (p.Actions.Count == 0) continue; // Empty actions → no permission, skipped (soft-deleted above)

                var existing = RolePermissions.FirstOrDefault(x => x.ResourceKey == p.ResourceKey && !x.IsDeleted);
                if (existing == null)
                {
                    existing = new RolePermission
                    {
                        RoleId = Id,
                        ResourceKey = p.ResourceKey,
                        AllowedFieldsJson = p.AllowedFieldsJson,
                        CreatedAt = DateTime.UtcNow
                    };
                    RolePermissions.Add(existing);
                }
                else
                {
                    existing.AllowedFieldsJson = p.AllowedFieldsJson;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                SyncActions(existing, p.Actions);
            }
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Syncs the Actions collection with the desired action list.
        /// Contextual actions (ManageMembers, etc.) also come through this path.
        /// The DB has a unique index on (RolePermissionId, Action), so previously
        /// soft-deleted rows are reactivated instead of being newly INSERTed.
        /// </summary>
        private static void SyncActions(RolePermission rp, IReadOnlyList<string> desired)
        {
            var desiredSet = desired.ToHashSet();

            // Soft-delete active rows that are no longer desired
            var toDelete = rp.Actions.Where(a => !a.IsDeleted && !desiredSet.Contains(a.Action)).ToList();
            foreach (var a in toDelete)
                a.MarkAsDeleted();

            // For desired ones: reactivate if inactive, add if missing
            foreach (var action in desired)
            {
                var existing = rp.Actions.FirstOrDefault(a => a.Action == action);
                if (existing == null)
                {
                    rp.Actions.Add(new RolePermissionAction
                    {
                        Action = action,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                // else: already active, leave untouched
            }
        }
    }
}
