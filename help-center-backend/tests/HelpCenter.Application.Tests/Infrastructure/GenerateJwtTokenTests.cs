using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Infrastructure.Services.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace HelpCenter.Application.Tests.Infrastructure;

public class GenerateJwtTokenTests
{
    private const string Secret = "super_secret_test_key_minimum_32_characters_long_123456";
    private readonly IConfiguration _config;
    private readonly IGenerateJwtToken _jwtGenerator;

    public GenerateJwtTokenTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", Secret },
            { "JwtSettings:Issuer", "HelpCenterIssuer" },
            { "JwtSettings:Audience", "HelpCenterAudience" }
        };

        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        _jwtGenerator = new GenerateJwtToken(_config);
    }

    [Fact]
    public void GenerateJwtToken_should_return_valid_signed_jwt()
    {
        var tokenString = _jwtGenerator.GenerateJwtToken("Admin", "admin_user", "5", "42");

        tokenString.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(tokenString, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
            ValidateIssuer = true,
            ValidIssuer = "HelpCenterIssuer",
            ValidateAudience = true,
            ValidAudience = "HelpCenterAudience",
            ValidateLifetime = true
        }, out var validatedToken);

        validatedToken.Should().NotBeNull();
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be("Admin");
        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be("admin_user");
        principal.FindFirst("CompanyId")?.Value.Should().Be("5");
        principal.FindFirst("UserId")?.Value.Should().Be("42");
    }
}
