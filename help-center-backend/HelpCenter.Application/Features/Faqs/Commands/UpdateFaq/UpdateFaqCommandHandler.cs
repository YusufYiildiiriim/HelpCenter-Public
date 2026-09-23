using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommandHandler : IRequestHandler<UpdateFaqCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateFaqRules _rules;

    public UpdateFaqCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateFaqRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await _unitOfWork.Repository<FAQ>().GetAsync(request.Id, cancellationToken);
        UpdateFaqRules.FaqShouldExist(faq);

        await _rules.FaqTitleShouldBeUniqueAsync(request.Title, request.Id, cancellationToken);
        await _rules.ProjectShouldExistAsync(request.ProjectId, cancellationToken);
        await _rules.ModuleShouldBelongToProjectAsync(request.ProjectId, request.ModuleId, cancellationToken);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(faq!, request.RowVersion);

        faq!.UpdateInfo(request.Title, request.Description, request.ProjectId, request.ModuleId, request.IsActive, request.IsPublic);

        await _unitOfWork.Repository<FAQ>().UpdateAsync(faq, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicFaqsPrefix);
        return true;
    }
}
