using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Commands.DeleteGuide;

public class DeleteGuideCommandHandler : IRequestHandler<DeleteGuideCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteGuideCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteGuideCommand request, CancellationToken cancellationToken)
    {
        var guideRepository = _unitOfWork.Repository<Guide>();
        var entity = await guideRepository.GetAsync(request.Id, cancellationToken);
        DeleteGuideRules.GuideShouldExist(entity);

        entity!.MarkAsDeleted();
        await guideRepository.UpdateAsync(entity);
        var result = await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        return result > 0;
    }
}
