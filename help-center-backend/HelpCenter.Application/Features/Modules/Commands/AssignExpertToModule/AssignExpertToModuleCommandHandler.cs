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
        var modules = await _unitOfWork.Repository<Module>()
            .FindAsync(x => x.Id == request.ModuleId, cancellationToken, x => x.ModuleExperts);

        var module = modules.FirstOrDefault();
        AssignExpertToModuleRules.ModuleShouldExist(module);

        var user = await _unitOfWork.Repository<User>().GetAsync(request.UserId, cancellationToken);
        AssignExpertToModuleRules.UserShouldExist(user);

        module!.AddExpert(request.UserId);

        await _unitOfWork.Repository<Module>().UpdateAsync(module, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
