using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;

public class RemoveUserFromProjectCommandHandler : IRequestHandler<RemoveUserFromProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RemoveUserFromProjectCommand request, CancellationToken cancellationToken)
    {
        var userProject = await _unitOfWork.Repository<UserProject>().GetAsync(request.Id, cancellationToken);
        RemoveUserFromProjectRules.ProjectAssignmentShouldExist(userProject);

        userProject!.IsDeleted = true;
        userProject.UpdatedAt = DateTime.Now;

        await _unitOfWork.Repository<UserProject>().UpdateAsync(userProject, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
