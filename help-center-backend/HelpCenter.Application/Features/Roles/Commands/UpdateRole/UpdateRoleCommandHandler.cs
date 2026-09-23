using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateRoleRules _rules;
    private readonly IAuditLogWriter _auditLog;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, UpdateRoleRules rules, IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Role>();
        var role = await repo.GetAsync(request.Id, cancellationToken);
        UpdateRoleRules.RoleShouldExist(role);

        await _rules.RoleNameShouldBeUniqueAsync(request.Name, request.Id, cancellationToken);

        var before = new { role!.Name, role.Description, role.IsActive };

        role.UpdateInfo(request.Name, request.Description, request.IsActive);

        // Concurrency token handling
        _unitOfWork.SetOriginalVersion(role, request.RowVersion);

        await repo.UpdateAsync(role, cancellationToken);
        var saved = await _unitOfWork.SaveAsync(cancellationToken) > 0;

        if (saved)
        {
            var after = new { role.Name, role.Description, role.IsActive };
            await _auditLog.WriteDiffAsync("RoleUpdated", "Role", role.Id, before, after, cancellationToken);
        }

        return saved;
    }
}
