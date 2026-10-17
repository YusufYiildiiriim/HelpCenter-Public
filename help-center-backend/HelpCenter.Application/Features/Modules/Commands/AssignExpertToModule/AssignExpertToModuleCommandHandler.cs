using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;

public class AssignExpertToModuleCommandHandler : IRequestHandler<AssignExpertToModuleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignExpertToModuleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignExpertToModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _unitOfWork.Repository<Module>()
            .FirstOrDefaultAsync(x => x.Id == request.ModuleId, asTracking: true, cancellationToken, x => x.ModuleExperts);
        AssignExpertToModuleRules.ModuleShouldExist(module);

        var user = await _unitOfWork.Repository<User>().GetAsync(request.UserId, cancellationToken);
        AssignExpertToModuleRules.UserShouldExist(user);

        module!.AddExpert(request.UserId);

        await _unitOfWork.Repository<Module>().UpdateAsync(module, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
