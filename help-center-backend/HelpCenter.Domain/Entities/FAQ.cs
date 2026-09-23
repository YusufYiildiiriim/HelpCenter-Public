namespace HelpCenter.Domain.Entities
{
    public class FAQ : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public int? ModuleId { get; set; }
        public virtual Module? Module { get; set; }

        public void UpdateInfo(string title, string description, int? projectId, int? moduleId, bool isActive, bool isPublic)
        {
            Title = title;
            Description = description;
            ProjectId = projectId;
            ModuleId = moduleId;
            IsActive = isActive;
            IsPublic = isPublic;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
