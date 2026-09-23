using HelpCenter.Application.Features.Roles.Commands;
using HelpCenter.Application.Features.Roles.Commands.CreateRole;
using HelpCenter.Application.Features.Roles.Commands.DeleteRole;
using HelpCenter.Application.Features.Roles.Commands.UpdateRole;
using HelpCenter.Application.Features.Roles.Commands.UpdateRolePermissions;
using HelpCenter.Application.Features.Roles.Queries;
using HelpCenter.Application.Features.Roles.Queries.GetRolePermissions;
using HelpCenter.Application.Features.Roles.Queries.GetRoles;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Authorize]
[Route("api/admin/role")]
[ApiController]
public class AdminRoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminRoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Roles, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null)
    {
        var query = new GetRolesQuery { Search = search };
        if (pageNumber.HasValue) query.PageNumber = pageNumber.Value;
        if (pageSize.HasValue) query.PageSize = pageSize.Value;

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("update")]
    [HasPermission(AppResources.Roles, PermissionActions.Update)]
    [EnableRateLimiting("rbac-mutation")]
    public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("get-permissions/{roleId}")]
    [HasPermission(AppResources.Roles, PermissionActions.Read)]
    public async Task<IActionResult> GetPermissions(int roleId)
    {
        var result = await _mediator.Send(new GetRolePermissionsQuery(roleId));
        return Ok(result);
    }

    [HttpPost("update-permissions")]
    [HasPermission(AppResources.Roles, PermissionActions.Update)]
    [EnableRateLimiting("rbac-mutation")]
    public async Task<IActionResult> UpdatePermissions([FromBody] UpdateRolePermissionsCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("create")]
    [HasPermission(AppResources.Roles, PermissionActions.Create)]
    [EnableRateLimiting("rbac-mutation")]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    [HasPermission(AppResources.Roles, PermissionActions.Delete)]
    [EnableRateLimiting("rbac-mutation")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteRoleCommand(id));
        return Ok(result);
    }
}
