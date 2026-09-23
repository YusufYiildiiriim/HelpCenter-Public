using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly IAuditLogWriter _auditLog;

    public DeleteUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserCommandHandler> logger,
        IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Kullanıcı silme işlemi başlatıldı. Kullanıcı ID: {UserId}", request.Id);

        var user = await _unitOfWork.Repository<User>().GetAsync(request.Id, cancellationToken);
        DeleteUserRules.UserShouldExist(user);

        user!.MarkAsDeleted();
        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await _auditLog.WriteAsync("UserDeleted", "User", user.Id, new { Email = user.Email, Username = user.Username }, cancellationToken);

        _logger.LogInformation("Kullanıcı başarıyla silindi (Soft Delete). Email: {Email}", user.Email);

        return true;
    }
}
