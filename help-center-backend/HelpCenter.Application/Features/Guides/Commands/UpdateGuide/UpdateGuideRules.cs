using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides.Commands.UpdateGuide;

/// <summary>
/// Rules belonging only to the update guide slice.
/// </summary>
public class UpdateGuideRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGuideRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

    /// <summary>
    /// Verifies the guide title is unique.
    /// </summary>
    public async Task GuideTitleShouldBeUniqueAsync(string title, int currentGuideId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Guide>()
            .FindAsync(g => g.Title == title && g.Id != currentGuideId, cancellationToken);

        if (existing.Any())
        {
            throw new GuideTitleAlreadyExistsException();
        }
    }
}
