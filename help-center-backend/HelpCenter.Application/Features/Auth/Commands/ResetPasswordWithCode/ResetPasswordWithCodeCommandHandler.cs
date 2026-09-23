using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ResetPasswordWithCode;

public class ResetPasswordWithCodeCommandHandler : IRequestHandler<ResetPasswordWithCodeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ICacheService _cache;

    public ResetPasswordWithCodeCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _cache = cache;
    }

    public async Task<bool> Handle(ResetPasswordWithCodeCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.EmailOrUsername?.Trim();
        ResetPasswordWithCodeRules.IdentifierShouldBeProvided(identifier);

        var cacheKey = $"otp_reset_{request.IsCustomer}_{identifier!.ToLowerInvariant()}";
        _cache.TryGetValue<string>(cacheKey, out var cachedCode);

        ResetPasswordWithCodeRules.ResetCodeShouldBeValid(cachedCode, request.Code);

        if (request.IsCustomer)
        {
            var customer = (await _unitOfWork.Repository<Customer>().FindAsync(
                x => x.Account.Email == identifier || x.Account.Username == identifier,
                cancellationToken,
                x => x.Account
            )).FirstOrDefault();

            ResetPasswordWithCodeRules.CustomerShouldExist(customer);

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

            ResetPasswordWithCodeRules.UserShouldExist(user);

            user!.Account.ChangePassword(_passwordService.HashPassword(request.NewPassword));
            user.Account.IsPasswordChangeRequired = false;
            user.Account.UpdatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Account>().UpdateAsync(user.Account, cancellationToken);
            await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        _cache.Remove(cacheKey);

        return true;
    }
}
