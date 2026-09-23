using System.Text.Json.Serialization;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Queries.CustomerLogin;

public class CustomerLoginQuery : IRequest<CustomerLoginResponse>
{
    [JsonPropertyName("email")]
    public string EmailOrUsername { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
