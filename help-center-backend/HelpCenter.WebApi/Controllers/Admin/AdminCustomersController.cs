using HelpCenter.Application.Features.Customers.Commands;
using HelpCenter.Application.Features.Customers.Commands.CreateCustomer;
using HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;
using HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;
using HelpCenter.Application.Features.Customers.Queries;
using HelpCenter.Application.Features.Customers.Queries.GetCustomerById;
using HelpCenter.Application.Features.Customers.Queries.GetCustomers;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/customers")]
[ApiController]
[Authorize]
public class AdminCustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all-customers")]
    [HasPermission(AppResources.Customers, PermissionActions.Read)]
    public async Task<IActionResult> GetAll(Guid companyPublicId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var response = await _mediator.Send(new GetCustomersQuery { CompanyPublicId = companyPublicId, PageNumber = pageNumber, PageSize = pageSize, Search = search });
        return Ok(response);
    }

    [HttpPost("add-customer")]
    [HasPermission(AppResources.Customers, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Add([FromBody] CreateCustomerCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("{publicId}")]
    [HasPermission(AppResources.Customers, PermissionActions.Read)]
    public async Task<IActionResult> GetById(Guid publicId)
    {
        var response = await _mediator.Send(new GetCustomerByIdQuery(publicId));
        return Ok(response);
    }

    [HttpPut("update-customer")]
    [HasPermission(AppResources.Customers, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update([FromBody] UpdateCustomerCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{publicId}")]
    [HasPermission(AppResources.Customers, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(Guid publicId)
    {
        var response = await _mediator.Send(new DeleteCustomerCommand(publicId));
        return Ok(response);
    }
}
