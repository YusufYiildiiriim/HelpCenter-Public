using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

/// <summary>
/// Rules belonging only to the assign-expert-to-module slice.
/// </summary>
public static class AssignExpertToModuleRules
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
    /// Verifies that the user to be assigned as an expert exists in the system.
    /// </summary>
    public static void UserShouldExist(User? user)
    {
        if (user == null || user.IsDeleted)
        {
            throw new UserNotFoundException();
        }
    }
}
