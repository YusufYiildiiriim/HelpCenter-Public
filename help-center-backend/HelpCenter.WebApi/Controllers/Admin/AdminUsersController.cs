using HelpCenter.Application.Features.Auth.Commands;
using HelpCenter.Application.Features.Auth.Commands.UserRegister;
using HelpCenter.Application.Features.Users.Commands;
using HelpCenter.Application.Features.Users.Commands.DeleteUser;
using HelpCenter.Application.Features.Users.Commands.UpdateUser;
using HelpCenter.Application.Features.Users.Queries;
using HelpCenter.Application.Features.Users.Queries.GetUsers;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/users")]
[ApiController]
[HasPermission(AppResources.Users, PermissionActions.Read)]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetUsersQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("add-user")]
    [HasPermission(AppResources.Users, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> AddUser([FromBody] UserRegisterCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [HasPermission(AppResources.Users, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest("ID uyuşmazlığı.");
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [HasPermission(AppResources.Users, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var response = await _mediator.Send(new DeleteUserCommand(id));
        return Ok(response);
    }
}
