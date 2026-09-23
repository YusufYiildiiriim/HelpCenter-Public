using System.Text.Json.Serialization;

namespace HelpCenter.Application.Features.Auth.Queries.CustomerLogin;

public class CustomerLoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTime RefreshTokenExpiresAt { get; set; }
    public Guid CompanyPublicId { get; set; }
    public Guid CustomerPublicId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsPasswordChangeRequired { get; set; }
}
