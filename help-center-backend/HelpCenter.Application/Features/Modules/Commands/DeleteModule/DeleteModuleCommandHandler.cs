using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Modules.Commands.DeleteModule;

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteModuleCommandHandler> _logger;
    private readonly ICacheService _cache;

    public DeleteModuleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteModuleCommandHandler> logger,
        ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Modül silme işlemi başlatıldı. Modül PublicId: {ModulePublicId}", request.PublicId);

        var module = await _unitOfWork.Repository<Module>().FirstOrDefaultAsync(
            x => x.PublicId == request.PublicId, asTracking: true, cancellationToken: cancellationToken);
        DeleteModuleRules.ModuleShouldExist(module);
        DeleteModuleRules.ModuleShouldNotBeLocked(module!);

        module!.MarkAsDeleted();
        await _unitOfWork.Repository<Module>().UpdateAsync(module, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.InvalidatePrefix(CacheKeys.PublicModulesPrefix);
        _cache.InvalidatePrefix(CacheKeys.PublicGuidesPrefix);

        _logger.LogInformation("Modül başarıyla silindi (Soft Delete). Modül: {ModuleName}", module.Name);

        return true;
    }
}
