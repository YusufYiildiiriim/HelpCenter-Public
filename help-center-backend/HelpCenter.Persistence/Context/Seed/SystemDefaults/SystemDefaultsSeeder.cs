using System.Reflection;
using System.Text.Json;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context.Seed.SystemDefaults;

/// <summary>Idempotently adds required system rows while preserving existing identifiers and edits.</summary>
public static class SystemDefaultsSeeder
{
    private sealed record SeedRow(string Name, JsonElement Values);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<int> SeedAsync(EfContext context, CancellationToken cancellationToken = default)
    {
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
            "HelpCenter.Persistence.Context.Seed.SystemDefaults.system-defaults.json")
            ?? throw new InvalidOperationException("Embedded system-defaults.json resource is missing.");
        var rows = await JsonSerializer.DeserializeAsync<List<SeedRow>>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("System defaults resource is empty or invalid.");
        // Seed IDs are only unique within an entity type. In particular, Roles and
        // RolePermissions both use the same source IDs (1..N), so a Guid-only map
        // lets permission rows overwrite the role mapping and produces invalid FKs.
        var ids = new Dictionary<(Type EntityType, Guid SourceId), Guid>();
        var inserted = 0;

        foreach (var row in rows)
        {
            var entityType = ResolveEntityType(row.Name);
            var entity = row.Values.Deserialize(entityType, JsonOptions)
                ?? throw new InvalidOperationException($"Could not deserialize system seed {row.Name}.");
            var sourceId = (Guid)(entityType.GetProperty(nameof(BaseEntity.Id))?.GetValue(entity)
                ?? throw new InvalidOperationException($"System seed {row.Name} has no Id."));
            RewriteSeedForeignKeys(entity, entityType, ids);
            var existing = await FindExistingAsync(context, entity, entityType, cancellationToken);
            if (existing is null)
            {
                context.Add(entity);
                await context.SaveChangesAsync(cancellationToken);
                existing = entity;
                inserted++;
            }
            var actualId = (Guid)(entityType.GetProperty(nameof(BaseEntity.Id))!.GetValue(existing)!);
            ids[(entityType, sourceId)] = actualId;
        }

        return inserted;
    }

    private static Type ResolveEntityType(string name) => name switch
    {
        nameof(Account) => typeof(Account),
        nameof(CustomerRequestStatus) => typeof(CustomerRequestStatus),
        nameof(MenuItem) => typeof(MenuItem),
        nameof(OrganizationInfo) => typeof(OrganizationInfo),
        nameof(Role) => typeof(Role),
        nameof(RolePermission) => typeof(RolePermission),
        nameof(RolePermissionAction) => typeof(RolePermissionAction),
        nameof(User) => typeof(User),
        nameof(UserRole) => typeof(UserRole),
        _ => throw new InvalidOperationException($"Unknown system seed entity '{name}'.")
    };

    private static void RewriteSeedForeignKeys(object entity, Type type, Dictionary<(Type EntityType, Guid SourceId), Guid> ids)
    {
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(Guid)))
        {
            if (property.Name is nameof(BaseEntity.Id) or nameof(BaseEntity.PublicId)) continue;
            var referencedType = GetReferencedEntityType(type, property.Name);
            if (referencedType is not null
                && property.GetValue(entity) is Guid oldId
                && ids.TryGetValue((referencedType, oldId), out var actualId))
                property.SetValue(entity, actualId);
        }
    }

    private static Type? GetReferencedEntityType(Type entityType, string propertyName)
    {
        if (entityType == typeof(RolePermission) && propertyName == nameof(RolePermission.RoleId))
            return typeof(Role);
        if (entityType == typeof(RolePermissionAction) && propertyName == nameof(RolePermissionAction.RolePermissionId))
            return typeof(RolePermission);
        if (entityType == typeof(UserRole))
        {
            if (propertyName == nameof(UserRole.RoleId)) return typeof(Role);
            if (propertyName == nameof(UserRole.UserId)) return typeof(User);
        }
        if (entityType == typeof(User) && propertyName == nameof(User.AccountId))
            return typeof(Account);
        if (entityType == typeof(MenuItem) && propertyName == nameof(MenuItem.ParentId))
            return typeof(MenuItem);

        return null;
    }

    private static async Task<object?> FindExistingAsync(EfContext context, object candidate, Type type, CancellationToken ct)
    {
        if (candidate is Account account)
            return await context.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Username == account.Username || x.Email == account.Email, ct);
        if (candidate is CustomerRequestStatus status)
            return await context.CustomerRequestStatuses.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == status.Id || x.Name == status.Name, ct);
        if (candidate is MenuItem menu)
            return await context.MenuItems.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == menu.Id || x.ResourceKey == menu.ResourceKey, ct);
        if (candidate is OrganizationInfo)
            return await context.OrganizationInfos.IgnoreQueryFilters().OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (candidate is Role role)
            return await context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == role.Id || x.Name == role.Name, ct);
        if (candidate is RolePermission permission)
            return await context.RolePermissions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RoleId == permission.RoleId && x.ResourceKey == permission.ResourceKey, ct);
        if (candidate is RolePermissionAction action)
            return await context.RolePermissionActions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.RolePermissionId == action.RolePermissionId && x.Action == action.Action, ct);
        if (candidate is User user)
            return await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.AccountId == user.AccountId, ct);
        if (candidate is UserRole userRole)
            return await context.UserRoles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserId == userRole.UserId && x.RoleId == userRole.RoleId, ct);
        throw new InvalidOperationException($"No natural-key matcher for {type.Name}.");
    }
}
