using HelpCenter.Application.Common.Security;
using System.Security.Claims;
using HelpCenter.Application.Features.Customers.Commands;
using HelpCenter.Application.Features.Customers.Commands.UpdateProfile;
using HelpCenter.Application.Features.Customers.Queries;
using HelpCenter.Application.Features.Customers.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/profile")]
[ApiController]
[Authorize]
public class CustomerProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentCustomerId
    {
        get
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idClaim, out var id)) return id;

            // Fallback to UserId claim if NameIdentifier is not an int (e.g. for admins)
            var userIdClaim = User.FindFirstValue(AppClaimTypes.UserId);
            if (Guid.TryParse(userIdClaim, out var userId)) return userId;

            return Guid.Empty;
        }
    }

    [HttpGet("get-my-profile")]
    public async Task<IActionResult> GetProfile()
    {
        var response = await _mediator.Send(new GetProfileQuery(CurrentCustomerId));
        return Ok(response);
    }

    [HttpPut("update-my-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        command.CustomerId = CurrentCustomerId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}
