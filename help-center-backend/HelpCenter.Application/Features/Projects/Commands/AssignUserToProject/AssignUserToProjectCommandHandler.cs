using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;

public class AssignUserToProjectCommandHandler : IRequestHandler<AssignUserToProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignUserToProjectCommand request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Repository<Project>()
            .FindAsync(x => x.Id == request.ProjectId, cancellationToken, x => x.UserProjects);

        var project = projects.FirstOrDefault();
        AssignUserToProjectRules.ProjectShouldExist(project);

        var user = await _unitOfWork.Repository<User>().GetAsync(request.UserId, cancellationToken);
        AssignUserToProjectRules.UserShouldExist(user);

        project!.AddUser(request.UserId);

        await _unitOfWork.Repository<Project>().UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return true;
    }
}
