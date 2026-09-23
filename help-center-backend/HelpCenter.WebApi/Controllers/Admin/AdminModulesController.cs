using HelpCenter.Application.Features.Modules.Commands;
using HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;
using HelpCenter.Application.Features.Modules.Commands.CreateModule;
using HelpCenter.Application.Features.Modules.Commands.DeleteModule;
using HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;
using HelpCenter.Application.Features.Modules.Commands.UpdateModule;
using HelpCenter.Application.Features.Modules.Queries;
using HelpCenter.Application.Features.Modules.Queries.GetAvailableExpertsForModule;
using HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;
using HelpCenter.Application.Features.Modules.Queries.GetModules;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/modules")]
[ApiController]
[Authorize]
public class AdminModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Modules, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null)
    {
        var query = new GetModulesQuery { OnlyActive = onlyActive, Search = search };
        if (pageNumber.HasValue) query.PageNumber = pageNumber.Value;
        if (pageSize.HasValue) query.PageSize = pageSize.Value;

        var response = await _mediator.Send(query);
        return Ok(response);
    }


    [HttpPost("add-module")]
    [HasPermission(AppResources.Modules, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Add([FromBody] CreateModuleCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{publicId}")]
    [HasPermission(AppResources.Modules, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update(Guid publicId, [FromBody] UpdateModuleCommand command)
    {
        command.PublicId = publicId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{publicId}")]
    [HasPermission(AppResources.Modules, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(Guid publicId)
    {
        var response = await _mediator.Send(new DeleteModuleCommand(publicId));
        return Ok(response);
    }

    // Lookup endpoint — minimal user list for the module expert picker.
    // Common to create and edit modes; independent of moduleId (pool-wide).
    // Requires Modules.ManageExperts instead of Users.Read.
    [HttpGet("available-experts")]
    [HasPermission(AppResources.Modules, PermissionActions.ManageExperts)]
    public async Task<IActionResult> AvailableExperts()
    {
        var response = await _mediator.Send(new GetAvailableExpertsForModuleQuery());
        return Ok(response);
    }
}
