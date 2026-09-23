using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.CreateModule;

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly CreateModuleRules _rules;

    public CreateModuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, CreateModuleRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        await _rules.ModuleNameShouldBeUniqueAsync(request.Name, null, cancellationToken);

        var module = Module.Create(request.Name, request.Description, request.IsActive);

        if (request.ExpertUserIds != null && request.ExpertUserIds.Any())
        {
            await _rules.UsersShouldExistAsync(request.ExpertUserIds, cancellationToken);
            module.SyncExperts(request.ExpertUserIds);
        }

        await _unitOfWork.Repository<Module>().AddAsync(module, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        return true;
    }
}
