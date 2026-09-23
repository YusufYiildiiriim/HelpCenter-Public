using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Rules belonging only to the role update slice.
/// </summary>
public class UpdateRoleRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the role exists in the system.
    /// </summary>
    public static void RoleShouldExist(Role? role)
    {
        if (role == null || role.IsDeleted)
        {
            throw new RoleNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the role name is unique.
    /// </summary>
    public async Task RoleNameShouldBeUniqueAsync(string name, int? currentRoleId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Role>()
            .FindAsync(r => r.Name == name && (currentRoleId == null || r.Id != currentRoleId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new RoleNameAlreadyExistsException();
        }
    }
}
