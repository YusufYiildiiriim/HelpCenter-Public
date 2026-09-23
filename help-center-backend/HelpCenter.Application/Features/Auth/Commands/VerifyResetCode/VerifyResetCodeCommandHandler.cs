using HelpCenter.Application.Interfaces;
using MediatR;

namespace HelpCenter.Application.Features.Auth.Commands.VerifyResetCode;

public class VerifyResetCodeCommandHandler : IRequestHandler<VerifyResetCodeCommand, bool>
{
    private readonly ICacheService _cache;

    public VerifyResetCodeCommandHandler(ICacheService cache)
    {
        _cache = cache;
    }

    public Task<bool> Handle(VerifyResetCodeCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.EmailOrUsername?.Trim();
        VerifyResetCodeRules.IdentifierShouldBeProvided(identifier);

        var cacheKey = $"otp_reset_{request.IsCustomer}_{identifier!.ToLowerInvariant()}";
        _cache.TryGetValue<string>(cacheKey, out var cachedCode);

        VerifyResetCodeRules.ResetCodeShouldBeValid(cachedCode, request.Code);

        return Task.FromResult(true);
    }
}
