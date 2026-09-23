using HelpCenter.Application.Features.Statuses.Queries;
using HelpCenter.Application.Features.Statuses.Queries.GetAllStatuses;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/statuses")]
[ApiController]
[Authorize]
public class AdminStatusController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminStatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Statuses, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllStatusesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
