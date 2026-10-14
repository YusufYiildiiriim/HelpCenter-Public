using HelpCenter.Application.Features.Users.Commands.UpdateUser;
using HelpCenter.WebApi.Controllers.Admin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.WebApi;

public sealed class AdminUsersControllerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(99)]
    public async Task UpdateUser_uses_route_id_even_when_body_id_is_missing_or_different(int bodyId)
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateUserCommand>(), Arg.Any<CancellationToken>()).Returns(true);
        var controller = new AdminUsersController(mediator);
        var command = new UpdateUserCommand { Id = bodyId, Name = "Name", Email = "user@example.test", RowVersion = [1, 2] };

        var result = await controller.UpdateUser(17, command);

        Assert.True(Assert.IsType<OkObjectResult>(result).Value is true);
        await mediator.Received(1).Send(Arg.Is<UpdateUserCommand>(sent =>
            sent.Id == 17 && sent.Name == "Name" && sent.Email == "user@example.test" && sent.RowVersion == command.RowVersion),
            Arg.Any<CancellationToken>());
    }
}
