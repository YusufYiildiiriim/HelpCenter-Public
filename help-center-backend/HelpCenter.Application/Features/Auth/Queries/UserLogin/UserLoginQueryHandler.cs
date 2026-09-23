using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Queries.UserLogin;

public class UserLoginQueryHandler : IRequestHandler<UserLoginQuery, UserLoginResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IAuthTokenService _tokenService;

    public UserLoginQueryHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IAuthTokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<UserLoginResponse> Handle(UserLoginQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Repository<User>()
            .FindAsync(x => x.Account.Email == request.Email || x.Account.Username == request.Email, cancellationToken, x => x.Account, x => x.UserRoles);

        var user = users.FirstOrDefault();
        UserLoginRules.UserShouldExist(user);
        UserLoginRules.UserShouldBeActive(user!);
        UserLoginRules.PasswordShouldMatch(_passwordService.VerifyPassword(request.Password, user!.Account.Password));

        var userRoles = await _unitOfWork.Repository<UserRole>()
            .FindAsync(ur => ur.UserId == user.Id, cancellationToken, ur => ur.Role);

        var roleNames = userRoles
            .Where(ur => ur.Role != null)
            .Select(ur => ur.Role.Name)
            .ToList();

        var primaryRole = roleNames.FirstOrDefault();
        UserLoginRules.UserShouldHaveRole(primaryRole);

        var tokens = await _tokenService.IssueTokensAsync(
            userId: user!.Id,
            role: primaryRole!,
            email: user.Account.Email,
            ipAddress: null,
            userAgent: null,
            ct: cancellationToken
        );

        return new UserLoginResponse
        {
            Token = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt,
            Expiration = tokens.AccessTokenExpiresAt,
            Role = primaryRole!,
            Username = user.Account.Username
        };
    }
}
