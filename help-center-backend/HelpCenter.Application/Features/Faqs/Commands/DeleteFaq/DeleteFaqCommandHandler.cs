using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;

public class DeleteFaqCommandHandler : IRequestHandler<DeleteFaqCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteFaqCommandHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<FAQ>();
        var faq = await repo.GetAsync(request.Id, cancellationToken);
        DeleteFaqRules.FaqShouldExist(faq);

        faq!.MarkAsDeleted();

        await repo.UpdateAsync(faq, cancellationToken);
        var affected = await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicFaqsPrefix);
        return affected > 0;
    }
}
