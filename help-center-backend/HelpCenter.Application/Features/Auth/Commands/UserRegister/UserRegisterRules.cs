using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Auth.Commands.UserRegister;

/// <summary>
/// Rules belonging only to the user registration slice.
/// </summary>
public class UserRegisterRules
{
    private readonly IUnitOfWork _unitOfWork;

    public UserRegisterRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Verifies the email address is unique.
    /// </summary>
    public async Task EmailShouldBeUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Repository<Account>()
            .AnyAsync(x => x.Email == email, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new EmailAlreadyExistsException();
        }
    }

    /// <summary>
    /// Verifies the username is unique.
    /// </summary>
    public async Task UsernameShouldBeUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Repository<Account>()
            .AnyAsync(x => x.Username == username, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new UsernameAlreadyExistsException();
        }
    }
}
