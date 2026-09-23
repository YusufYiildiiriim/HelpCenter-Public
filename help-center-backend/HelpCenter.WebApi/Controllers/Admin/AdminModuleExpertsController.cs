using HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;
using HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;
using HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/module-experts")]
[ApiController]
[Authorize]
public class AdminModuleExpertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminModuleExpertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{moduleId}")]
    [HasPermission(AppResources.Modules, PermissionActions.Read)]
    public async Task<IActionResult> GetByModuleId(int moduleId)
    {
        var response = await _mediator.Send(new GetExpertsByModuleIdQuery(moduleId));
        return Ok(response);
    }

    [HttpPost("assign")]
    [HasPermission(AppResources.Modules, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Assign([FromBody] AssignExpertToModuleCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{expertId}")]
    [HasPermission(AppResources.Modules, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Remove(int expertId)
    {
        var response = await _mediator.Send(new RemoveExpertFromModuleCommand(expertId));
        return Ok(response);
    }
}
