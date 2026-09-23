using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly IAuditLogWriter _auditLog;
    private readonly UpdateUserRules _rules;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ILogger<UpdateUserCommandHandler> logger,
        IAuditLogWriter auditLog,
        UpdateUserRules rules)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _logger = logger;
        _auditLog = auditLog;
        _rules = rules;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Kullanıcı güncelleniyor. ID: {UserId}", request.Id);

        var userList = await _unitOfWork.Repository<User>().FindAsync(
            x => x.Id == request.Id,
            cancellationToken,
            x => x.Account,
            x => x.UserRoles);

        var user = userList.FirstOrDefault();
        UpdateUserRules.UserShouldExist(user);

        if (request.RowVersion != null)
            _unitOfWork.SetOriginalVersion(user!, request.RowVersion);

        var before = new
        {
            user!.FirstName,
            user.LastName,
            user.Email,
            user.Username,
            user.IsActive,
            RoleIds = user.UserRoles.Select(r => r.RoleId).ToList()
        };

        await _rules.EmailShouldBeUniqueAsync(request.Email, user!.AccountId, cancellationToken);
        if (request.Username != null)
        {
            await _rules.UsernameShouldBeUniqueAsync(request.Username, user.AccountId, cancellationToken);
        }

        user.UpdateInfo(request.Name, request.LastName, request.Email, request.IsActive);
        if (request.Username != null)
        {
            user.Username = request.Username;
        }

        if (request.RoleIds != null)
        {
            await _unitOfWork.Repository<UserRole>()
                .ExecuteDeleteAsync(ur => ur.UserId == request.Id, cancellationToken);

            user.SyncRoles(request.RoleIds);
        }

        user.Account.UpdatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Account>().UpdateAsync(user.Account, cancellationToken);
        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        var after = new
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Username = user.Username,
            IsActive = user.IsActive,
            RoleIds = request.RoleIds ?? before.RoleIds
        };

        await _auditLog.WriteDiffAsync("UserUpdated", "User", user.Id, before, after, cancellationToken);

        _logger.LogInformation("Kullanıcı başarıyla güncellendi. Email: {Email}", user.Account.Email);
        return true;
    }
}
