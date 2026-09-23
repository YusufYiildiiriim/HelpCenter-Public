using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpCenter.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HelpCenter.Infrastructure.Services.Jwt;

public class GenerateJwtToken : IGenerateJwtToken
{
    private readonly IConfiguration _configuration;
    public GenerateJwtToken(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    string IGenerateJwtToken.GenerateJwtToken(string role, string username, string? companyId, string? userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new List<Claim>
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrEmpty(companyId))
        {
            claims.Add(new Claim("CompanyId", companyId));
        }

        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim("UserId", userId));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
