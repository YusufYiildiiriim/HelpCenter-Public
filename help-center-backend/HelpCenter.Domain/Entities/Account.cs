using System;

namespace HelpCenter.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsPasswordChangeRequired { get; set; } = false;
        public string? ProfilePicture { get; set; }

        // Navigation properties for one-to-one relationship
        public virtual User? User { get; set; }
        public virtual Customer? Customer { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Generates a unique-safe username from first+last name
        public static string GenerateUsername(string firstName, string lastName)
        {
            var normalized = $"{firstName.Trim().ToLowerInvariant()}.{lastName.Trim().ToLowerInvariant()}";
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9.]", "");
            return normalized;
        }

        // Rich Domain Logic
        public void UpdateInfo(string firstName, string lastName, string? email = null, string? phoneNumber = null)
        {
            FirstName = firstName;
            LastName = lastName;
            if (email != null) Email = email;
            if (phoneNumber != null) PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPassword)
        {
            Password = newPassword;
            IsPasswordChangeRequired = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RequirePasswordChange()
        {
            IsPasswordChangeRequired = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
