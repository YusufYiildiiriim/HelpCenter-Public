using HelpCenter.Application.Features.RequestSubjects.Commands;
using HelpCenter.Application.Features.RequestSubjects.Commands.CreateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Commands.DeleteRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Commands.UpdateRequestSubject;
using HelpCenter.Application.Features.RequestSubjects.Queries;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/subjects")]
[ApiController]
[Authorize]
public class AdminRequestSubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminRequestSubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Subjects, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetRequestSubjectsQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("add")]
    [HasPermission(AppResources.Subjects, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Add([FromBody] CreateRequestSubjectCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("update")]
    [HasPermission(AppResources.Subjects, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update([FromBody] UpdateRequestSubjectCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("delete/{id}")]
    [HasPermission(AppResources.Subjects, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _mediator.Send(new DeleteRequestSubjectCommand(id));
        return Ok(response);
    }
}
