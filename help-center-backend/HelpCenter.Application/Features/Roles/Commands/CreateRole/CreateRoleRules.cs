using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Rules belonging only to the role creation slice.
/// </summary>
public class CreateRoleRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
