using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly UpdateModuleRules _rules;

    public UpdateModuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cache, UpdateModuleRules rules)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var modules = await _unitOfWork.Repository<Module>()
            .FindAsync(x => x.PublicId == request.PublicId, cancellationToken, x => x.ModuleExperts);

        var module = modules.FirstOrDefault();
        UpdateModuleRules.ModuleShouldExist(module);
        UpdateModuleRules.ModuleShouldNotBeLocked(module!);

        await _rules.ModuleNameShouldBeUniqueAsync(request.Name, module!.Id, cancellationToken);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(module!, request.RowVersion);

        module!.UpdateInfo(request.Name, request.Description, request.IsActive);

        if (request.ExpertUserIds != null)
        {
            if (request.ExpertUserIds.Any())
            {
                await _rules.UsersShouldExistAsync(request.ExpertUserIds, cancellationToken);
            }

            module.SyncExperts(request.ExpertUserIds);
        }

        await _unitOfWork.Repository<Module>().UpdateAsync(module!, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);
        return true;
    }
}
