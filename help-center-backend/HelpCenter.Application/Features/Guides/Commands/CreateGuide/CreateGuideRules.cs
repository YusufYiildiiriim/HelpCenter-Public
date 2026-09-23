using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Guides.Commands.CreateGuide;

/// <summary>
/// Rules belonging only to the create guide slice.
/// </summary>
public class CreateGuideRules
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateGuideRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the guide title is unique.
    /// </summary>
    public async Task GuideTitleShouldBeUniqueAsync(string title, int? currentGuideId, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Repository<Guide>()
            .FindAsync(g => g.Title == title && (currentGuideId == null || g.Id != currentGuideId.Value), cancellationToken);

        if (existing.Any())
        {
            throw new GuideTitleAlreadyExistsException();
        }
    }
}
