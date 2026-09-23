using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.Email?.Trim();

        if (request.IsCustomer)
        {
            var customer = (await _unitOfWork.Repository<Customer>().FindAsync(
                x => x.Account.Email == identifier || x.Account.Username == identifier,
                cancellationToken,
                x => x.Account
            )).FirstOrDefault();

            ChangePasswordRules.CustomerShouldExist(customer);

            customer!.Account.ChangePassword(_passwordService.HashPassword(request.NewPassword));
            customer.Account.IsPasswordChangeRequired = false;
            customer.Account.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Account>().UpdateAsync(customer.Account, cancellationToken);
            await _unitOfWork.Repository<Customer>().UpdateAsync(customer, cancellationToken);
        }
        else
        {
            var user = (await _unitOfWork.Repository<User>().FindAsync(
                x => x.Account.Email == identifier || x.Account.Username == identifier,
                cancellationToken,
                x => x.Account
            )).FirstOrDefault();

            ChangePasswordRules.UserShouldExist(user);

            user!.Account.ChangePassword(_passwordService.HashPassword(request.NewPassword));
            user.Account.IsPasswordChangeRequired = false;
            user.Account.UpdatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Account>().UpdateAsync(user.Account, cancellationToken);
            await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveAsync(cancellationToken);
        return true;
    }
}
