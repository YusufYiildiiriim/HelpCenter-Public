namespace HelpCenter.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<Company> Companies { get; set; } = new HashSet<Company>();
    public virtual ICollection<UserProject> UserProjects { get; set; } = new HashSet<UserProject>();
    public virtual ICollection<ProjectModule> ProjectModules { get; set; } = new HashSet<ProjectModule>();

    // Rich Domain Logic
    public void UpdateInfo(string name, string description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncUsers(IEnumerable<int> userIds)
    {
        var toRemove = UserProjects.Where(x => !userIds.Contains(x.UserId) && !x.IsDeleted).ToList();
        foreach (var userProject in toRemove)
        {
            userProject.IsDeleted = true;
            userProject.UpdatedAt = DateTime.UtcNow;
        }

        var existingUserIds = UserProjects.Where(x => !x.IsDeleted).Select(x => x.UserId).ToList();
        foreach (var userId in userIds)
        {
            if (!existingUserIds.Contains(userId))
            {
                UserProjects.Add(new UserProject
                {
                    ProjectId = Id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    public void AddUser(int userId)
    {
        if (!UserProjects.Any(x => x.UserId == userId && !x.IsDeleted))
        {
            UserProjects.Add(new UserProject
            {
                ProjectId = Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SyncModules(IEnumerable<int> moduleIds)
    {
        foreach (var existing in ProjectModules.Where(pm => !pm.IsDeleted))
        {
            existing.MarkAsDeleted();
        }
        foreach (var moduleId in moduleIds)
        {
            ProjectModules.Add(new ProjectModule { ModuleId = moduleId });
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public static Project Create(string name, string description, bool isActive = true)
    {
        return new Project
        {
            Name = name,
            Description = description,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }
}
