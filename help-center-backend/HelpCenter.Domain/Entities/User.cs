namespace HelpCenter.Domain.Entities
{
    public class User : BaseEntity
    {
        public int AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();

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

        public string Username
        {
            get => Account?.Username ?? string.Empty;
            set { if (Account != null) Account.Username = value; }
        }

        public string FullName => Account?.FullName ?? string.Empty;

        // Rich Domain Logic
        public static User Register(string email, string firstName, string lastName, string hashedPassword, string username)
        {
            return new User
            {
                Account = new Account
                {
                    Email = email,
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName,
                    Password = hashedPassword,
                    IsPasswordChangeRequired = true,
                    CreatedAt = DateTime.UtcNow
                },
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateInfo(string firstName, string lastName, string email, bool isActive)
        {
            Account.UpdateInfo(firstName, lastName, email);
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string hashedPassword)
        {
            Account.ChangePassword(hashedPassword);
            UpdatedAt = DateTime.UtcNow;
        }

        public void SyncRoles(IEnumerable<int> roleIds)
        {
            // Simple approach: Clear and re-add for UserRoles (assuming it's a join table without extra data)
            UserRoles.Clear();
            foreach (var roleId in roleIds)
            {
                UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddRoles(IEnumerable<int> roleIds)
        {
            foreach (var roleId in roleIds)
            {
                UserRoles.Add(new UserRole { RoleId = roleId });
            }
        }
    }
}
