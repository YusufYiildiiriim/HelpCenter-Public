namespace HelpCenter.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public int AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<CustomerRequest> CustomerRequests { get; set; } = new HashSet<CustomerRequest>();

        // Shortcut properties for backward compatibility
        public string Email
        {
            get => Account?.Email ?? string.Empty;
            set { if (Account != null) Account.Email = value; }
        }

        public string FirstName
        {
            get => Account?.FirstName ?? string.Empty;
            set { if (Account != null) Account.FirstName = value; }
        }

        public string LastName
        {
            get => Account?.LastName ?? string.Empty;
            set { if (Account != null) Account.LastName = value; }
        }

        public string Password
        {
            get => Account?.Password ?? string.Empty;
            set { if (Account != null) Account.Password = value; }
        }

        public bool IsPasswordChangeRequired
        {
            get => Account?.IsPasswordChangeRequired ?? false;
            set { if (Account != null) Account.IsPasswordChangeRequired = value; }
        }

        public string PhoneNumber
        {
            get => Account?.PhoneNumber ?? string.Empty;
            set { if (Account != null) Account.PhoneNumber = value; }
        }

        public string FullName => Account?.FullName ?? string.Empty;

        // Rich Domain Logic
        public static Customer Create(int companyId, string firstName, string lastName, string email, string phone, string hashedPassword, string username)
        {
            return new Customer
            {
                CompanyId = companyId,
                Account = new Account
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Username = username,
                    PhoneNumber = phone,
                    Password = hashedPassword,
                    IsPasswordChangeRequired = true,
                    CreatedAt = DateTime.UtcNow
                },
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateCompany(int companyId)
        {
            CompanyId = companyId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
