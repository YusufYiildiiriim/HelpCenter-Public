using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides.Commands.DeleteGuide;

/// <summary>
/// Rules belonging only to the delete guide slice.
/// </summary>
public static class DeleteGuideRules
{
    /// <summary>
    /// Verifies the guide exists in the system.
    /// </summary>
    public static void GuideShouldExist(Guide? guide)
    {
        if (guide == null || guide.IsDeleted)
        {
            throw new GuideNotFoundException();
        }
    }
}
