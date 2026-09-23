using FluentAssertions;
using HelpCenter.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Behaviors;

public class LoggingBehaviorTests
{
    public record SampleLogCommand(string Data) : IRequest<string>;

    [Fact]
    public async Task Handle_should_log_and_return_response_on_success()
    {
        var behavior = new LoggingBehavior<SampleLogCommand, string>(
            NullLogger<LoggingBehavior<SampleLogCommand, string>>.Instance);

        var command = new SampleLogCommand("hello");
        RequestHandlerDelegate<string> next = _ => Task.FromResult("ok");
        var result = await behavior.Handle(command, next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_should_log_error_and_rethrow_on_exception()
    {
        var behavior = new LoggingBehavior<SampleLogCommand, string>(
            NullLogger<LoggingBehavior<SampleLogCommand, string>>.Instance);

        var command = new SampleLogCommand("fail");
        RequestHandlerDelegate<string> next = _ => throw new InvalidOperationException("boom");
        Func<Task> act = async () => await behavior.Handle(command, next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
