using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
