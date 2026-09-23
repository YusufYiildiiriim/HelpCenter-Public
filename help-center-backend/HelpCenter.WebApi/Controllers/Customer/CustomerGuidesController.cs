using HelpCenter.Application.Features.Guides.Queries;
using HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/guides")]
[ApiController]
[Authorize]
public class CustomerGuidesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerGuidesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetCustomerGuidesQuery());
        return Ok(response);
    }
}
