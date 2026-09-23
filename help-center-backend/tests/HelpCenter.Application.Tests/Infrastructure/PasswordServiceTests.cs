using FluentAssertions;
using HelpCenter.Infrastructure.Services;
using Xunit;

namespace HelpCenter.Application.Tests.Infrastructure;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void HashPassword_should_produce_valid_argon2id_hash_string()
    {
        var password = "SecurePassword123!";
        var hash = _service.HashPassword(password);

        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$argon2id$v=19$m=65536,t=3,p=4$");
    }

    [Fact]
    public void VerifyPassword_should_return_true_for_matching_argon2id_password()
    {
        var password = "MySecretPassword!";
        var hash = _service.HashPassword(password);

        var isValid = _service.VerifyPassword(password, hash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_should_return_false_for_wrong_password()
    {
        var hash = _service.HashPassword("CorrectPassword1!");

        var isValid = _service.VerifyPassword("WrongPassword1!", hash);

        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "some_hash")]
    [InlineData("pwd", "")]
    [InlineData(null, "some_hash")]
    [InlineData("pwd", null)]
    public void VerifyPassword_should_return_false_for_null_or_empty_inputs(string? password, string? hash)
    {
        var isValid = _service.VerifyPassword(password!, hash!);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_should_support_legacy_bcrypt_hash()
    {
        var password = "BcryptPassword123";
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword(password);

        var isValid = _service.VerifyPassword(password, bcryptHash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void NeedsRehash_should_return_true_for_legacy_bcrypt_hash()
    {
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword("legacy_password");

        var needsRehash = _service.NeedsRehash(bcryptHash);

        needsRehash.Should().BeTrue();
    }

    [Fact]
    public void NeedsRehash_should_return_false_for_current_argon2id_hash()
    {
        var hash = _service.HashPassword("current_password");

        var needsRehash = _service.NeedsRehash(hash);

        needsRehash.Should().BeFalse();
    }
}
