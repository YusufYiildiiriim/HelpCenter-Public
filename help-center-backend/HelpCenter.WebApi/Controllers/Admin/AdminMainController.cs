using HelpCenter.Application.Features.Menu.Queries.GetMenuItems;
using HelpCenter.Application.Features.Statistics.Queries;
using HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;
using HelpCenter.Application.Features.Statistics.Queries.GetAdminStatistics;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize]
public class AdminMainController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminMainController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("statistics")]
    [HasPermission(AppResources.Dashboard, PermissionActions.Read)]
    [EnableRateLimiting("heavy")]
    public async Task<IActionResult> GetStatistics()
    {
        var response = await _mediator.Send(new GetAdminStatisticsQuery());
        return Ok(response);
    }

    [HttpGet("reports")]
    [HasPermission(AppResources.Dashboard, PermissionActions.Read)]
    [EnableRateLimiting("heavy")]
    public async Task<IActionResult> GetReports()
    {
        var response = await _mediator.Send(new GetAdminReportsQuery());
        return Ok(response);
    }

    [HttpGet("menu-items")]
    public async Task<IActionResult> GetMenuItems()
    {
        var response = await _mediator.Send(new GetMenuItemsQuery());
        return Ok(response);
    }
}
