using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Features.Modules.Queries.GetModules;
using HelpCenter.WebApi.Controllers.Admin;
using MediatR;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public sealed class AdminModulesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly AdminModulesController _controller;

    public AdminModulesControllerTests()
    {
        _controller = new AdminModulesController(_mediator);
        _mediator.Send(Arg.Any<GetModulesQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<ModuleDto>());
    }

    [Fact]
    public async Task GetAll_should_preserve_the_query_default_when_onlyActive_is_omitted()
    {
        await _controller.GetAll();

        var query = GetSentQuery();

        Assert.True(query.OnlyActive);
    }

    [Fact]
    public async Task GetAll_should_allow_an_explicit_onlyActive_false_filter()
    {
        await _controller.GetAll(onlyActive: false);

        var query = GetSentQuery();

        Assert.False(query.OnlyActive);
    }

    private GetModulesQuery GetSentQuery() => (GetModulesQuery)_mediator.ReceivedCalls()
        .Single(call => call.GetMethodInfo().Name == nameof(IMediator.Send))
        .GetArguments()[0]!;
}
