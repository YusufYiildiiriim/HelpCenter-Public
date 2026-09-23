namespace HelpCenter.Domain.Entities
{
    public class RequestSubject : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsLocked { get; set; } = false;

        public int? ModuleId { get; set; }
        public virtual Module? Module { get; set; }

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public virtual ICollection<CustomerRequest> CustomerRequests { get; set; } = new HashSet<CustomerRequest>();

        // Rich Domain Logic
        public static RequestSubject Create(string name, string description, int? moduleId, bool isActive = true)
        {
            return new RequestSubject
            {
                Name = name,
                Description = description,
                ModuleId = moduleId,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Lock enforcement for request subject update/delete happens in the Application layer
        // (UpdateRequestSubjectRules / DeleteRequestSubjectRules) before the handler is invoked —
        // since the Domain cannot depend on Application's ConflictException derivatives,
        // no invariant is enforced here.
        public void UpdateInfo(string name, string description, int? moduleId, bool isActive)
        {
            Name = name;
            Description = description;
            ModuleId = moduleId;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
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
    }
}
