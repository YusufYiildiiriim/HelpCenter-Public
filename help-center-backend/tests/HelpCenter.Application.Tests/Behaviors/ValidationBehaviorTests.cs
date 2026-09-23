using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HelpCenter.Application.Behaviors;
using MediatR;
using NSubstitute;
using Xunit;
using ApplicationValidationException = HelpCenter.Application.Exceptions.ValidationException;

namespace HelpCenter.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public record SampleCommand(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_should_call_next_when_no_validators_exist()
    {
        var behavior = new ValidationBehavior<SampleCommand, string>(Enumerable.Empty<IValidator<SampleCommand>>());
        var command = new SampleCommand("test");
        var nextCalled = false;
        RequestHandlerDelegate<string> next1 = _ =>
        {
            nextCalled = true;
            return Task.FromResult("success");
        };

        var result = await behavior.Handle(command, next1, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("success");
    }

    [Fact]
    public async Task Handle_should_call_next_when_validators_pass()
    {
        var validator = Substitute.For<IValidator<SampleCommand>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<SampleCommand, string>(new[] { validator });
        var command = new SampleCommand("valid");
        RequestHandlerDelegate<string> next = _ => Task.FromResult("passed");

        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.Should().Be("passed");
    }

    [Fact]
    public async Task Handle_should_throw_ValidationException_when_validation_fails()
    {
        var validator = Substitute.For<IValidator<SampleCommand>>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name is too short")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<SampleCommand, string>(new[] { validator });
        var command = new SampleCommand("");
        RequestHandlerDelegate<string> next = _ => Task.FromResult("should not run");

        Func<Task> act = async () => await behavior.Handle(command, next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ApplicationValidationException>();
        ex.Which.Errors.Should().Contain("Name is required");
        ex.Which.Errors.Should().Contain("Name is too short");
    }
}
