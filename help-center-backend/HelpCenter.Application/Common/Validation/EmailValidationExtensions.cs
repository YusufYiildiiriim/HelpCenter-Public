using FluentValidation;
using HelpCenter.Domain.ValueObjects;

namespace HelpCenter.Application.Common.Validation;

public static class EmailValidationExtensions
{
    // Required/optional semantics and field messages belong to the caller.
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => string.IsNullOrEmpty(value) || Email.IsValid(value));
}