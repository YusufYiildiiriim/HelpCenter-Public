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

    private int CurrentCustomerId
    {
        get
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idClaim, out int id)) return id;

            // Fallback to UserId claim if NameIdentifier is not an int (e.g. for admins)
            var userIdClaim = User.FindFirstValue("UserId");
            if (int.TryParse(userIdClaim, out int userId)) return userId;

            return 0;
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
