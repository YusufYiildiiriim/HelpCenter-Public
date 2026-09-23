using HelpCenter.Application.Features.Projects.Commands;
using HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;
using HelpCenter.Application.Features.Projects.Commands.CreateProject;
using HelpCenter.Application.Features.Projects.Commands.DeleteProject;
using HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;
using HelpCenter.Application.Features.Projects.Commands.UpdateProject;
using HelpCenter.Application.Features.Projects.Queries;
using HelpCenter.Application.Features.Projects.Queries.GetAvailableModulesForProject;
using HelpCenter.Application.Features.Projects.Queries.GetAvailableUsersForProject;
using HelpCenter.Application.Features.Projects.Queries.GetProjects;
using HelpCenter.Application.Features.Projects.Queries.GetUsersByProjectId;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/projects")]
[ApiController]
[Authorize]
public class AdminProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Projects, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000, [FromQuery] string? search = null)
    {
        var response = await _mediator.Send(new GetProjectsQuery
        {
            OnlyActive = onlyActive,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });
        return Ok(response);
    }

    [HttpPost("add-project")]
    [HasPermission(AppResources.Projects, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Add([FromBody] CreateProjectCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{publicId}")]
    [HasPermission(AppResources.Projects, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update(Guid publicId, [FromBody] UpdateProjectCommand command)
    {
        command.PublicId = publicId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{publicId}")]
    [HasPermission(AppResources.Projects, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(Guid publicId)
    {
        var response = await _mediator.Send(new DeleteProjectCommand(publicId));
        return Ok(response);
    }

    [HttpGet("{projectId}/users")]
    [HasPermission(AppResources.Projects, PermissionActions.Read)]
    public async Task<IActionResult> GetUsers(int projectId)
    {
        var response = await _mediator.Send(new GetUsersByProjectIdQuery(projectId));
        return Ok(response);
    }

    [HttpPost("assign-user")]
    [HasPermission(AppResources.Projects, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> AssignUser([FromBody] AssignUserToProjectCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("remove-user/{id}")]
    [HasPermission(AppResources.Projects, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var response = await _mediator.Send(new RemoveUserFromProjectCommand(id));
        return Ok(response);
    }

    // Lookup endpoint — minimal user list for the project member picker.
    // Common to create and edit modes; requires Projects.ManageMembers instead of Users.Read.
    [HttpGet("available-users")]
    [HasPermission(AppResources.Projects, PermissionActions.ManageMembers)]
    public async Task<IActionResult> AvailableUsers()
    {
        var response = await _mediator.Send(new GetAvailableUsersForProjectQuery());
        return Ok(response);
    }

    // Lookup endpoint — minimal module list for the project module picker.
    [HttpGet("available-modules")]
    [HasPermission(AppResources.Projects, PermissionActions.ManageModules)]
    public async Task<IActionResult> AvailableModules()
    {
        var response = await _mediator.Send(new GetAvailableModulesForProjectQuery());
        return Ok(response);
    }
}
