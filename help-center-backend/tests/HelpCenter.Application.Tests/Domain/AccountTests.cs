using FluentAssertions;
using HelpCenter.Domain.Entities;
using Xunit;

namespace HelpCenter.Application.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void GenerateUsername_should_normalize_and_combine_first_and_last_names()
    {
        var username = Account.GenerateUsername("John", "Doe");
        username.Should().Be("john.doe");
    }

    [Fact]
    public void GenerateUsername_should_strip_special_characters_and_whitespace()
    {
        var username = Account.GenerateUsername("  Ahmet!@#  ", " Yılmaz$$ ");
        username.Should().Be("ahmet.ylmaz");
    }

    [Fact]
    public void FullName_should_combine_first_and_last_names()
    {
        var account = new Account { FirstName = "Jane", LastName = "Doe" };
        account.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void UpdateInfo_should_update_properties_and_touch_updated_at()
    {
        var account = new Account
        {
            FirstName = "OldFirst",
            LastName = "OldLast",
            Email = "old@test.com",
            PhoneNumber = "111"
        };

        account.UpdateInfo("NewFirst", "NewLast", "new@test.com", "222");

        account.FirstName.Should().Be("NewFirst");
        account.LastName.Should().Be("NewLast");
        account.Email.Should().Be("new@test.com");
        account.PhoneNumber.Should().Be("222");
        account.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangePassword_should_update_password_and_clear_required_flag()
    {
        var account = new Account
        {
            Password = "OldHashedPassword",
            IsPasswordChangeRequired = true
        };

        account.ChangePassword("NewHashedPassword");

        account.Password.Should().Be("NewHashedPassword");
        account.IsPasswordChangeRequired.Should().BeFalse();
        account.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RequirePasswordChange_should_set_flag_to_true()
    {
        var account = new Account { IsPasswordChangeRequired = false };

        account.RequirePasswordChange();

        account.IsPasswordChangeRequired.Should().BeTrue();
        account.UpdatedAt.Should().NotBeNull();
    }
}
