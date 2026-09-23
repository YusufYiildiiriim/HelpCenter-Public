using HelpCenter.Application.Features.RequestSubjects.Queries;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/subjects")]
[ApiController]
[Authorize]
public class CustomerRequestSubjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerRequestSubjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetCustomerRequestSubjectsQuery());
        return Ok(response);
    }
}
