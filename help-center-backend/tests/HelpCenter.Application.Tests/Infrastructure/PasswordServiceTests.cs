using FluentAssertions;
using HelpCenter.Infrastructure.Services;
using HelpCenter.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HelpCenter.Infrastructure;
using Xunit;

namespace HelpCenter.Application.Tests.Infrastructure;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void Configured_parameters_round_trip_and_old_hashes_remain_verifiable()
    {
        var old = new PasswordService(Options.Create(new PasswordHashOptions { MemorySizeKb = 8192, Iterations = 1, Parallelism = 1 }));
        var current = new PasswordService(Options.Create(new PasswordHashOptions { MemorySizeKb = 8192, Iterations = 2, Parallelism = 1 }));
        var hash = old.HashPassword("Test password");
        current.VerifyPassword("Test password", hash).Should().BeTrue();
        current.NeedsRehash(hash).Should().BeTrue();
        var upgraded = current.HashPassword("Test password");
        upgraded.Should().StartWith("$argon2id$v=19$m=8192,t=2,p=1$");
        current.NeedsRehash(upgraded).Should().BeFalse();
        current.VerifyPassword("Test password", upgraded).Should().BeTrue();
    }

    [Theory]
    [InlineData("$argon2id$v=16$m=8192,t=1,p=1$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=-1,t=1,p=1$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=8192,t=0,p=1$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=8192,t=1,p=0$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=8192,t=1$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=8192,t=1,p=1,m=8192$c2FsdHNhbHQ=$aGFzaA==")]
    [InlineData("$argon2id$v=19$m=2147483647,t=1,p=1$invalid$invalid")]
    public void Malformed_hash_is_rejected_without_throwing(string encoded)
    {
        _service.VerifyPassword("password", encoded).Should().BeFalse();
        _service.NeedsRehash(encoded).Should().BeTrue();
    }

    [Theory]
    [InlineData("MemorySizeKb", "0")]
    [InlineData("Iterations", "0")]
    [InlineData("Parallelism", "0")]
    public void Invalid_configured_parameters_fail_options_validation(string key, string value)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"PasswordHashing:{key}"] = value
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddInfrastructureServices();
        using var provider = services.BuildServiceProvider();
        ((Action)(() => _ = provider.GetRequiredService<IOptions<PasswordHashOptions>>().Value))
            .Should().Throw<OptionsValidationException>();
    }

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
