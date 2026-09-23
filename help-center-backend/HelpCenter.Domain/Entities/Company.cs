namespace HelpCenter.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;

        // Authorized Person Info
        public string ContactPersonName { get; set; } = string.Empty;
        public string ContactPersonSurname { get; set; } = string.Empty;
        public string ContactPersonEmail { get; set; } = string.Empty;
        public string ContactPersonPhone { get; set; } = string.Empty;
        public string ContactPersonUsername { get; set; } = string.Empty;

        public bool IsDemoActive { get; set; } = false;
        public int BranchCount { get; set; }
        public string PreviousSystem { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        public virtual ICollection<CompanyModule> CompanyModules { get; set; } = new HashSet<CompanyModule>();
        public virtual ICollection<Customer> Users { get; set; } = new HashSet<Customer>();

        // Rich Domain Logic
        public void UpdateInfo(string name, string address, string phone, string mail)
        {
            Name = name;
            Address = address;
            Phone = phone;
            Mail = mail;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateContactPerson(string name, string surname, string email, string phone, string username = "")
        {
            ContactPersonName = name;
            ContactPersonSurname = surname;
            ContactPersonEmail = email;
            ContactPersonPhone = phone;
            if (!string.IsNullOrWhiteSpace(username))
                ContactPersonUsername = username;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SyncModules(IEnumerable<int> moduleIds)
        {
            foreach (var existing in CompanyModules.Where(cm => !cm.IsDeleted))
            {
                existing.MarkAsDeleted();
            }
            foreach (var moduleId in moduleIds)
            {
                CompanyModules.Add(new CompanyModule { ModuleId = moduleId });
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProject(int? projectId)
        {
            ProjectId = projectId;
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
