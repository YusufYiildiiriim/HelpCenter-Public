using FluentAssertions;
using HelpCenter.Application.Features.Companies.Commands.UpdateCompany;
using HelpCenter.WebApi.Controllers.Admin;
using MediatR;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public class AdminCompaniesControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly AdminCompaniesController _controller;

    public AdminCompaniesControllerTests()
    {
        _controller = new AdminCompaniesController(_mediator);
        _mediator.Send(Arg.Any<UpdateCompanyCommand>(), Arg.Any<CancellationToken>()).Returns(true);
    }

    private static AdminCompaniesController.UpdateCompanyRequest BuildRequest(Guid rowVersion) => new(
        Name: "Acme",
        Address: "Somewhere",
        Phone: "555",
        Mail: "acme@example.com",
        ContactPersonName: "A",
        ContactPersonSurname: "B",
        ContactPersonEmail: "a.b@example.com",
        ContactPersonPhone: "555",
        IsDemoActive: false,
        BranchCount: 1,
        ModuleIds: new List<Guid>(),
        PreviousSystem: "None",
        RowVersion: rowVersion);

    private async Task<UpdateCompanyCommand> CaptureSentCommandAsync(Guid rowVersion)
    {
        await _controller.Update(Guid.NewGuid(), BuildRequest(rowVersion));

        return (UpdateCompanyCommand)_mediator.ReceivedCalls()
            .Single(call => call.GetMethodInfo().Name == nameof(IMediator.Send))
            .GetArguments()[0]!;
    }

    [Fact]
    public async Task Update_should_preserve_empty_guid_for_the_concurrency_check()
    {
        var command = await CaptureSentCommandAsync(rowVersion: Guid.Empty);

        command.RowVersion.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task Update_should_map_guid_RowVersion()
    {
        var version = Guid.NewGuid();
        var command = await CaptureSentCommandAsync(version);

        command.RowVersion.Should().Be(version);
    }
}
