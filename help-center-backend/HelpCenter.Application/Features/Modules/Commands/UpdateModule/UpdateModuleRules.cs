using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Commands.UpdateModule;

/// <summary>
/// Rules belonging only to the module-update slice.
/// </summary>
public class UpdateModuleRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateModuleRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the module exists.
    /// </summary>
    public static void ModuleShouldExist(Module? module)
    {
        if (module == null || module.IsDeleted)
        {
            throw new ModuleNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the module is not locked.
    /// </summary>
    public static void ModuleShouldNotBeLocked(Module module)
    {
        if (module.IsLocked)
        {
            throw new ModuleLockedException();
        }
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
