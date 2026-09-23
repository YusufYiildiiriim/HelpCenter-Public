using HelpCenter.Application.Features.Guides.Commands;
using HelpCenter.Application.Features.Guides.Commands.CreateGuide;
using HelpCenter.Application.Features.Guides.Commands.DeleteGuide;
using HelpCenter.Application.Features.Guides.Commands.UpdateGuide;
using HelpCenter.Application.Features.Guides.Queries;
using HelpCenter.Application.Features.Guides.Queries.GetGuides;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/guides")]
[ApiController]
[Authorize]
public class AdminGuidesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminGuidesController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet("get-all")]
    [HasPermission(AppResources.Guide, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetGuidesQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("add")]
    [HasPermission(AppResources.Guide, PermissionActions.Create)]
    [EnableRateLimiting("upload")]
    public async Task<IActionResult> Create([FromForm] CreateGuideCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("update")]
    [HasPermission(AppResources.Guide, PermissionActions.Update)]
    [EnableRateLimiting("upload")]
    public async Task<IActionResult> Update([FromForm] UpdateGuideCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("delete/{id}")]
    [HasPermission(AppResources.Guide, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _mediator.Send(new DeleteGuideCommand(id));
        return Ok(response);
    }
}
