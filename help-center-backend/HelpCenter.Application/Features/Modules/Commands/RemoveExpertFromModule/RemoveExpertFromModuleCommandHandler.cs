using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;

public class RemoveExpertFromModuleCommandHandler : IRequestHandler<RemoveExpertFromModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveExpertFromModuleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RemoveExpertFromModuleCommand request, CancellationToken cancellationToken)
    {
        var expert = await _unitOfWork.Repository<ModuleExpert>().GetAsync(request.ExpertId, cancellationToken);
        RemoveExpertFromModuleRules.ModuleExpertShouldExist(expert);

        expert!.IsDeleted = true;
        expert.UpdatedAt = DateTime.Now;

        await _unitOfWork.Repository<ModuleExpert>().UpdateAsync(expert, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
