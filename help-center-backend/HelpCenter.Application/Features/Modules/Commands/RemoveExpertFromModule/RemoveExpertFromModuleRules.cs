using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

/// <summary>
/// Rules belonging only to the remove-expert-from-module slice.
/// </summary>
public static class RemoveExpertFromModuleRules
{
    /// <summary>
    /// Verifies that the module-expert assignment exists.
    /// </summary>
    public static void ModuleExpertShouldExist(ModuleExpert? moduleExpert)
    {
        if (moduleExpert == null)
        {
            throw new ModuleExpertNotFoundException();
        }
    }
}
