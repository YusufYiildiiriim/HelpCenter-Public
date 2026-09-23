using System.Security.Cryptography;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cache;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _cache = cache;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.EmailOrUsername?.Trim();
        ForgotPasswordRules.IdentifierShouldBeProvided(identifier);

        string? targetEmail = null;
        string? userName = null;

        if (request.IsCustomer)
        {
            var customer = (await _unitOfWork.Repository<Customer>().FindAsync(
                x => x.Account.Email == identifier || x.Account.Username == identifier,
                cancellationToken,
                x => x.Account
            )).FirstOrDefault();

            ForgotPasswordRules.CustomerShouldExist(customer);

            targetEmail = customer!.Account.Email;
            userName = customer.Account.FullName ?? customer.Account.Username ?? customer.Account.Email;
        }
        else
        {
            var user = (await _unitOfWork.Repository<User>().FindAsync(
                x => x.Account.Email == identifier || x.Account.Username == identifier,
                cancellationToken,
                x => x.Account
            )).FirstOrDefault();

            ForgotPasswordRules.UserShouldExist(user);

            targetEmail = user!.Account.Email;
            userName = user.Account.FullName ?? user.Account.Username ?? user.Account.Email;
        }

        ForgotPasswordRules.AccountShouldHaveEmail(targetEmail);

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var codeLifetime = TimeSpan.FromMinutes(15);

        var cacheKey = $"otp_reset_{request.IsCustomer}_{identifier!.ToLowerInvariant()}";
        _cache.Set(cacheKey, code, codeLifetime);

        if (!string.Equals(identifier, targetEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailCacheKey = $"otp_reset_{request.IsCustomer}_{targetEmail.ToLowerInvariant()}";
            _cache.Set(emailCacheKey, code, codeLifetime);
        }

        await _emailService.SendPasswordResetCodeAsync(targetEmail!, userName, code, cancellationToken);

        return true;
    }
}
