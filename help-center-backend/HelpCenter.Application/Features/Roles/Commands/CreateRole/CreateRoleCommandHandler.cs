using AutoMapper;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateRoleCommandHandler> _logger;
    private readonly CreateRoleRules _rules;
    private readonly IAuditLogWriter _auditLog;

    public CreateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateRoleCommandHandler> logger,
        CreateRoleRules rules,
        IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _rules = rules;
        _auditLog = auditLog;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni rol oluşturuluyor: {RoleName}", request.Name);

        await _rules.RoleNameShouldBeUniqueAsync(request.Name, null, cancellationToken);

        var role = Role.Create(request.Name, request.Description);

        // Baseline seed: Dashboard.Read
        role.RolePermissions.Add(new RolePermission
        {
            Role = role,
            ResourceKey = "Dashboard",
            Actions = new List<RolePermissionAction>
            {
                new RolePermissionAction { Action = "Read" }
            }
        });

        await _unitOfWork.Repository<Role>().AddAsync(role, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await _auditLog.WriteAsync("RoleCreated", "Role", role.Id, new { role.Name, role.Description }, cancellationToken);

        _logger.LogInformation("Rol başarıyla oluşturuldu: {RoleName}", request.Name);

        return _mapper.Map<RoleDto>(role);
    }
}
