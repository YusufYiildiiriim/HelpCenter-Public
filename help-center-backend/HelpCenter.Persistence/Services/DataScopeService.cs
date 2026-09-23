using System.Text.Json;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Persistence.Services;

public class DataScopeService : IDataScopeService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserContext _userContext;
    private int? _cachedUserId;
    private List<int>? _cachedRoleIds;
    private readonly Dictionary<string, HashSet<string>?> _allowedFieldsCache = new();
    private readonly Dictionary<string, bool> _permissionCache = new();

    public DataScopeService(IUnitOfWork uow, IUserContext userContext)
    {
        _uow = uow;
        _userContext = userContext;
    }

    public int CurrentUserId
    {
        get
        {
            var id = _userContext.UserId;
            if (id > 0)
            {
                _cachedUserId = id;
                return id;
            }
            return _cachedUserId ?? 0;
        }
    }

    private async Task<List<int>> GetRoleIdsAsync(CancellationToken ct)
    {
        if (_cachedRoleIds != null) return _cachedRoleIds;

        var userRoles = await _uow.Repository<UserRole>()
            .FindAsync(ur => ur.UserId == CurrentUserId && !ur.IsDeleted, ct);

        _cachedRoleIds = userRoles.Select(ur => ur.RoleId).ToList();
        return _cachedRoleIds;
    }

    public async Task<HashSet<string>?> GetAllowedFieldsAsync(string resourceKey, CancellationToken ct = default)
    {
        if (_allowedFieldsCache.TryGetValue(resourceKey, out var cached)) return cached;

        var roleIds = await GetRoleIdsAsync(ct);

        var perms = await _uow.Repository<RolePermission>()
            .FindAsync(rp => roleIds.Contains(rp.RoleId)
                          && rp.ResourceKey == resourceKey
                          && !rp.IsDeleted, ct);

        // No permission defined means no restriction
        if (!perms.Any())
        {
            _allowedFieldsCache[resourceKey] = null;
            return null;
        }

        // If none of them has AllowedFieldsJson, all fields are open
        if (perms.All(p => string.IsNullOrWhiteSpace(p.AllowedFieldsJson)))
        {
            _allowedFieldsCache[resourceKey] = null;
            return null;
        }

        // Merge the fields allowed by all roles (union)
        var merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var perm in perms)
        {
            if (string.IsNullOrWhiteSpace(perm.AllowedFieldsJson)) continue;
            try
            {
                var fields = JsonSerializer.Deserialize<List<string>>(perm.AllowedFieldsJson);
                if (fields != null)
                    foreach (var f in fields) merged.Add(f);
            }
            catch { /* malformed JSON — skip this role's contribution */ }
        }

        _allowedFieldsCache[resourceKey] = merged;
        return merged;
    }

    public async Task<bool> HasPermissionAsync(string resourceKey, string action, CancellationToken ct = default)
    {
        var cacheKey = $"{resourceKey}.{action}";
        if (_permissionCache.TryGetValue(cacheKey, out var cached)) return cached;

        var roleIds = await GetRoleIdsAsync(ct);

        var perms = await _uow.Repository<RolePermission>().FindAsync(
            rp => roleIds.Contains(rp.RoleId)
                  && rp.ResourceKey == resourceKey
                  && !rp.IsDeleted,
            ct,
            rp => rp.Actions);

        var result = perms.Any(p => p.Actions.Any(a => !a.IsDeleted && a.Action == action));

        _permissionCache[cacheKey] = result;
        return result;
    }
}
