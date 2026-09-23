using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Commands.DeleteModule;

/// <summary>
/// Rules belonging only to the module-deletion slice.
/// </summary>
public static class DeleteModuleRules
{
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
}
