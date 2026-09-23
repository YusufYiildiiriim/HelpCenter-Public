using System.Text.Json.Serialization;

namespace HelpCenter.Application.Features.Auth.Queries.UserLogin;

public class UserLoginResponse
{
    public string Token { get; set; } = string.Empty;
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTime RefreshTokenExpiresAt { get; set; }
    public DateTime Expiration { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
