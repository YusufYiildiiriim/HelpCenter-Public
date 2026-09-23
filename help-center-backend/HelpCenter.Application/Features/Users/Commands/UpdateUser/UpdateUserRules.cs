using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Rules belonging only to the user update slice.
/// </summary>
public class UpdateUserRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies that the user exists in the system.
    /// </summary>
    public static void UserShouldExist(User? user)
    {
        if (user == null || user.IsDeleted)
        {
            throw new UserNotFoundException();
        }
    }

    /// <summary>
    /// Verifies that the email address is not used by another account (excluding the user's own account).
    /// </summary>
    public async Task EmailShouldBeUniqueAsync(string email, int currentAccountId, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Repository<Account>()
            .AnyAsync(x => x.Email == email && x.Id != currentAccountId, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new EmailAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies that the username is not used by another account (excluding the user's own account).
    /// </summary>
    public async Task UsernameShouldBeUniqueAsync(string username, int currentAccountId, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Repository<Account>()
            .AnyAsync(x => x.Username == username && x.Id != currentAccountId, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new UsernameAlreadyExistsException();
        }
    }
}
