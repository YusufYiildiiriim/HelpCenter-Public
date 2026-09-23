using HelpCenter.Application.Features.Faqs.Commands;
using HelpCenter.Application.Features.Faqs.Commands.CreateFaq;
using HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;
using HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;
using HelpCenter.Application.Features.Faqs.Queries;
using HelpCenter.Application.Features.Faqs.Queries.GetFaqs;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/faq")]
[ApiController]
[Authorize]
public class AdminFaqController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminFaqController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.FAQ, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetFaqsQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }


    [HttpPost("create-faq")]
    [HasPermission(AppResources.FAQ, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Create([FromBody] CreateFaqCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("update-faq")]
    [HasPermission(AppResources.FAQ, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update([FromBody] UpdateFaqCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("delete-faq/{id}")]
    [HasPermission(AppResources.FAQ, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _mediator.Send(new DeleteFaqCommand(id));
        return Ok(response);
    }
}
