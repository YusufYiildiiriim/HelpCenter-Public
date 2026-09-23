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

    private static AdminCompaniesController.UpdateCompanyRequest BuildRequest(string? rowVersion) => new(
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
        ModuleIds: new List<int>(),
        PreviousSystem: "None",
        RowVersion: rowVersion);

    private async Task<UpdateCompanyCommand> CaptureSentCommandAsync(string? rowVersion)
    {
        await _controller.Update(Guid.NewGuid(), BuildRequest(rowVersion));

        return (UpdateCompanyCommand)_mediator.ReceivedCalls()
            .Single(call => call.GetMethodInfo().Name == nameof(IMediator.Send))
            .GetArguments()[0]!;
    }

    [Fact]
    public async Task Update_should_map_missing_RowVersion_to_null_not_empty_array()
    {
        // Regression test: a client that omits RowVersion entirely must not trigger a
        // concurrency check. Array.Empty<byte>() passes the handler's "!= null" guard and
        // makes EF issue "UPDATE ... WHERE RowVersion = 0x", which never matches any row and
        // throws a bogus DbUpdateConcurrencyException.
        var command = await CaptureSentCommandAsync(rowVersion: null);

        command.RowVersion.Should().BeNull();
    }

    [Fact]
    public async Task Update_should_map_empty_RowVersion_string_to_null()
    {
        var command = await CaptureSentCommandAsync(rowVersion: string.Empty);

        command.RowVersion.Should().BeNull();
    }

    [Fact]
    public async Task Update_should_map_malformed_Base64_RowVersion_to_null()
    {
        var command = await CaptureSentCommandAsync(rowVersion: "not-valid-base64!!");

        command.RowVersion.Should().BeNull();
    }

    [Fact]
    public async Task Update_should_decode_valid_Base64_RowVersion()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var command = await CaptureSentCommandAsync(rowVersion: Convert.ToBase64String(bytes));

        command.RowVersion.Should().BeEquivalentTo(bytes);
    }
}
