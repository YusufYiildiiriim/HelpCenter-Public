using HelpCenter.Application.Features.Faqs.Queries;
using HelpCenter.Application.Features.Faqs.Queries.GetPublicFaqs;
using HelpCenter.Application.Features.Guides.Queries;
using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using HelpCenter.Application.Features.Modules.Queries;
using HelpCenter.Application.Features.Modules.Queries.GetPublicModules;
using HelpCenter.Application.Features.Organization.Queries;
using HelpCenter.Application.Features.Organization.Queries.GetPublicOrganizationInfo;
using HelpCenter.Application.Features.Projects.Queries;
using HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetPublicRequestSubjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Common;

[Route("api/public")]
[ApiController]
[EnableRateLimiting("public")]
public class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        var response = await _mediator.Send(new GetPublicProjectsQuery());
        return Ok(response);
    }

    [HttpGet("faq")]
    public async Task<IActionResult> GetFaqs(
        [FromQuery] int? projectId = null,
        [FromQuery] int? moduleId = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        var response = await _mediator.Send(new GetPublicFaqsQuery(projectId, moduleId, search, pageNumber, pageSize));
        return Ok(response);
    }

    [HttpGet("modules")]
    public async Task<IActionResult> GetModules([FromQuery] int? projectId = null)
    {
        var response = await _mediator.Send(new GetPublicModulesQuery(projectId));
        return Ok(response);
    }

    [HttpGet("guides")]
    public async Task<IActionResult> GetGuides(
        [FromQuery] int? projectId = null,
        [FromQuery] string? module = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        var response = await _mediator.Send(new GetPublicGuidesQuery(projectId, module, search, pageNumber, pageSize));
        return Ok(response);
    }

    [HttpGet("organization")]
    public async Task<IActionResult> GetOrganizationInfo()
    {
        var response = await _mediator.Send(new GetPublicOrganizationInfoQuery());
        return Ok(response);
    }
}
