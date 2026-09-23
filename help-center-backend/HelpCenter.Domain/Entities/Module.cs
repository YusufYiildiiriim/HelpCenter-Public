namespace HelpCenter.Domain.Entities;

public class Module : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsLocked { get; set; } = false;
    public virtual ICollection<CompanyModule> CompanyModules { get; set; } = new HashSet<CompanyModule>();
    public virtual ICollection<ModuleExpert> ModuleExperts { get; set; } = new HashSet<ModuleExpert>();
    public virtual ICollection<ProjectModule> ProjectModules { get; set; } = new HashSet<ProjectModule>();

    // Rich Domain Logic
    // Lock enforcement for module update/delete happens in the Application layer
    // (UpdateModuleRules / DeleteModuleRules) before the handler is invoked — since the Domain
    // cannot depend on Application's ConflictException derivatives, no invariant is enforced here.
    public void UpdateInfo(string name, string description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncExperts(IEnumerable<int> userIds)
    {
        // Remove experts not in the new list
        var toRemove = ModuleExperts.Where(x => !userIds.Contains(x.UserId) && !x.IsDeleted).ToList();
        foreach (var expert in toRemove)
        {
            expert.IsDeleted = true;
            expert.UpdatedAt = DateTime.UtcNow;
        }

        // Add new experts
        var existingUserIds = ModuleExperts.Where(x => !x.IsDeleted).Select(x => x.UserId).ToList();
        foreach (var userId in userIds)
        {
            if (!existingUserIds.Contains(userId))
            {
                ModuleExperts.Add(new ModuleExpert
                {
                    ModuleId = Id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    public void AddExpert(int userId)
    {
        if (!ModuleExperts.Any(x => x.UserId == userId && !x.IsDeleted))
        {
            ModuleExperts.Add(new ModuleExpert
            {
                ModuleId = Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Lock()
    {
        IsLocked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unlock()
    {
        IsLocked = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Module Create(string name, string description, bool isActive = true)
    {
        return new Module
        {
            Name = name,
            Description = description,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }
}
