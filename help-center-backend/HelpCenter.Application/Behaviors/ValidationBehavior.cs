using FluentValidation;
using MediatR;
using ApplicationValidationException = HelpCenter.Application.Exceptions.ValidationException;

namespace HelpCenter.Application.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for the incoming request before
/// the handler. If validation fails, throws <see cref="ApplicationValidationException"/>;
/// this exception is turned into a 400 response with the error list by the
/// GlobalExceptionHandler. Requests without validators pass through at no cost.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => f.ErrorMessage)
            .Distinct()
            .ToList();

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException("Doğrulama hatası.", errors);
        }

        return await next();
    }
}
