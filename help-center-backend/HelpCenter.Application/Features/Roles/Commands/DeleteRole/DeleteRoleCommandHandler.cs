using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteRoleCommandHandler> _logger;
    private readonly IAuditLogWriter _auditLog;

    public DeleteRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteRoleCommandHandler> logger,
        IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Rol silme işlemi başlatıldı. Rol ID: {RoleId}", request.Id);

        var roleRepo = _unitOfWork.Repository<Role>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();

        var role = await roleRepo.GetAsync(request.Id, cancellationToken);
        DeleteRoleRules.RoleShouldExist(role);

        // Check if any user is assigned to this role
        var isAssigned = await userRoleRepo.AnyAsync(x => x.RoleId == request.Id, cancellationToken: cancellationToken);
        if (isAssigned)
        {
            _logger.LogError("Rol silinemedi: Kullanıcılara atanmış. Rol: {RoleName}", role!.Name);
        }

        DeleteRoleRules.RoleShouldNotBeAssignedToUsers(isAssigned);

        // Soft delete
        role!.MarkAsDeleted();

        await roleRepo.UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await _auditLog.WriteAsync("RoleDeleted", "Role", role.Id, new { role.Name }, cancellationToken);

        _logger.LogInformation("Rol başarıyla silindi (Soft Delete). Rol: {RoleName}", role.Name);

        return true;
    }
}
