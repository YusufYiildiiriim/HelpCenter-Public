using MediatR;

namespace HelpCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public List<int>? RoleIds { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }
}
