using HelpCenter.Application.Features.Faqs.Queries;
using HelpCenter.Application.Features.Faqs.Queries.GetCustomerFaqs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/faq")]
[ApiController]
[AllowAnonymous]
public class CustomerFaqController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerFaqController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all-faq")]
    public async Task<IActionResult> GetAll([FromQuery] int? projectId, [FromQuery] int? moduleId)
    {
        var response = await _mediator.Send(new GetCustomerFaqsQuery(projectId, moduleId));
        return Ok(response);
    }
}
