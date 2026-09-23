using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Commands.CreateModule;

/// <summary>
/// Rules belonging only to the module-creation slice.
/// </summary>
public class CreateModuleRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateModuleRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the module name is unique.
    /// </summary>
    public async Task ModuleNameShouldBeUniqueAsync(string name, int? currentModuleId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Module>()
            .FindAsync(m => m.Name == name && (currentModuleId == null || m.Id != currentModuleId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new ModuleNameAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies that all users to be assigned as experts exist in the system.
    /// </summary>
    public async Task UsersShouldExistAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
    {
        var distinctIds = userIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return;
        }

        var existingCount = await _unitOfWork.Repository<User>()
            .CountAsync(u => distinctIds.Contains(u.Id) && !u.IsDeleted, cancellationToken);

        if (existingCount != distinctIds.Count)
        {
            throw new UserNotFoundException();
        }
    }
}
