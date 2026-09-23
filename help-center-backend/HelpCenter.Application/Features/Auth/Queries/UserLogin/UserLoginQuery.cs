using MediatR;

namespace HelpCenter.Application.Features.Auth.Queries.UserLogin;

public class UserLoginQuery : IRequest<UserLoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
