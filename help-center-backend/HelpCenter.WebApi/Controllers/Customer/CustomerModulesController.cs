using HelpCenter.Application.Common.Security;
using HelpCenter.Application.Features.Modules.Queries;
using HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/modules")]
[ApiController]
[Authorize]
public class CustomerModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerModulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all-modules")]
    public async Task<IActionResult> GetAll()
    {
        var userIdStr = User.FindFirst(AppClaimTypes.UserId)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var companyIdStr = User.FindFirst(AppClaimTypes.CompanyId)?.Value;

        Guid.TryParse(userIdStr, out var customerId);
        Guid.TryParse(companyIdStr, out var companyId);

        var response = await _mediator.Send(new GetCustomerModulesQuery
        {
            CustomerId = customerId,
            CompanyId = companyId
        });
        return Ok(response);
    }
}
