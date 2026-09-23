using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.UserRegister;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly UserRegisterRules _rules;
    private readonly IAuditLogWriter _auditLog;

    public UserRegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        UserRegisterRules rules,
        IAuditLogWriter auditLog)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _rules = rules;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        await _rules.EmailShouldBeUniqueAsync(request.Email, cancellationToken);

        var username = await ResolveUniqueUsernameAsync(request.Username, request.Name, request.LastName, cancellationToken);

        var user = User.Register(
            request.Email,
            request.Name,
            request.LastName,
            _passwordService.HashPassword(request.Password),
            username
        );

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            user.AddRoles(request.RoleIds);
        }

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await _auditLog.WriteAsync("UserRegistered", "User", user.Id, new
        {
            Email = request.Email,
            Username = username,
            FullName = $"{request.Name} {request.LastName}".Trim(),
            RoleIds = request.RoleIds
        }, cancellationToken);

        return true;
    }

    private async Task<string> ResolveUniqueUsernameAsync(string? requested, string firstName, string lastName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var normalized = Account.GenerateUsername(requested, "").TrimEnd('.');
            await _rules.UsernameShouldBeUniqueAsync(normalized, cancellationToken);
            return normalized;
        }

        var baseUsername = Account.GenerateUsername(firstName, lastName);
        var candidate = baseUsername;
        var suffix = 1;

        while (await _unitOfWork.Repository<Account>().AnyAsync(x => x.Username == candidate, cancellationToken: cancellationToken))
        {
            candidate = $"{baseUsername}{suffix}";
            suffix++;
        }

        return candidate;
    }
}
