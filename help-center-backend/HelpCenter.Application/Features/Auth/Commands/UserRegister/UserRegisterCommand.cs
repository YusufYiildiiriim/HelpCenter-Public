using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.UserRegister;

public class UserRegisterCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int>? RoleIds { get; set; }
    public string? Username { get; set; }
}
