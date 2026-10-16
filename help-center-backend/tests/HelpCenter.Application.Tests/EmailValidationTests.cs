using HelpCenter.Application.Features.Auth.Commands.UserRegister;
using HelpCenter.Domain.ValueObjects;
using Xunit;

namespace HelpCenter.Application.Tests;

public class EmailValidationTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData(" USER+tag@example.com ", true)]
    [InlineData("user@localhost", false)]
    [InlineData("user name@example.com", false)]
    [InlineData("user@@example.com", false)]
    [InlineData("@example.com", false)]
    [InlineData("user@example.", false)]
    public void Domain_and_command_share_email_policy(string value, bool valid)
    {
        Assert.Equal(valid, Email.IsValid(value));
        var result = new UserRegisterCommandValidator().Validate(new UserRegisterCommand { Email = value });
        Assert.Equal(!valid, result.Errors.Any(error => error.PropertyName == "Email"));
        if (valid) Assert.Equal(value.Trim().ToLowerInvariant(), Email.Create(value).Value);
        else Assert.Throws<ArgumentException>(() => Email.Create(value));
    }

    [Fact]
    public void Rejects_addresses_exceeding_storage_limit()
    {
        var value = new string('a', 243) + "@example.com";
        Assert.False(Email.IsValid(value));
        Assert.Contains(new UserRegisterCommandValidator().Validate(new UserRegisterCommand { Email = value }).Errors,
            error => error.PropertyName == "Email");
    }

    [Fact]
    public void Empty_required_field_keeps_existing_message()
    {
        var result = new UserRegisterCommandValidator().Validate(new UserRegisterCommand { Email = "" });
        Assert.Contains(result.Errors, error => error.PropertyName == "Email" && error.ErrorMessage == "E-posta alanı boş bırakılamaz.");
    }
}