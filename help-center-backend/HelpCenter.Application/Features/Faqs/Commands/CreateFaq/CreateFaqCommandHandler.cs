using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.CreateFaq;

public class CreateFaqCommandHandler : IRequestHandler<CreateFaqCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly CreateFaqRules _rules;

    public CreateFaqCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache, CreateFaqRules rules)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        await _rules.FaqTitleShouldBeUniqueAsync(request.Title, null, cancellationToken);
        await _rules.ProjectShouldExistAsync(request.ProjectId, cancellationToken);
        await _rules.ModuleShouldBelongToProjectAsync(request.ProjectId, request.ModuleId, cancellationToken);

        var faq = _mapper.Map<FAQ>(request);
        faq.ProjectId = request.ProjectId;
        faq.ModuleId = request.ModuleId;
        faq.IsActive = request.IsActive;
        faq.IsPublic = request.IsPublic;

        await _unitOfWork.Repository<FAQ>().AddAsync(faq, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicFaqsPrefix);
        return true;
    }
}
