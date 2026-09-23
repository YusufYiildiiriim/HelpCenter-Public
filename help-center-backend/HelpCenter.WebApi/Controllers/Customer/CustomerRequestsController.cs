using System.Security.Claims;
using HelpCenter.Application.Features.Requests.Commands;
using HelpCenter.Application.Features.Requests.Commands.CloseRequest;
using HelpCenter.Application.Features.Requests.Commands.CreateRequest;
using HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;
using HelpCenter.Application.Features.Requests.Commands.SendMessage;
using HelpCenter.Application.Features.Requests.Queries;
using HelpCenter.Application.Features.Requests.Queries.GetCustomerMessages;
using HelpCenter.Application.Features.Requests.Queries.GetCustomerRequestById;
using HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Customer;

[Route("api/customer/requests")]
[ApiController]
[Authorize]
public class CustomerRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerRequestsController(IMediator mediator)
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

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests([FromQuery] string? status)
    {
        var response = await _mediator.Send(new GetCustomerRequestsQuery
        {
            CustomerId = CurrentCustomerId,
            Status = status
        });
        return Ok(response);
    }

    [HttpGet("get-by-id/{publicId}")]
    public async Task<IActionResult> GetById(Guid publicId)
    {
        var response = await _mediator.Send(new GetCustomerRequestByIdQuery { PublicId = publicId, CustomerId = CurrentCustomerId });
        return Ok(response);
    }

    [HttpGet("{requestPublicId}/messages")]
    public async Task<IActionResult> GetMessages(Guid requestPublicId)
    {
        var response = await _mediator.Send(new GetCustomerMessagesQuery { RequestPublicId = requestPublicId, CustomerId = CurrentCustomerId });
        return Ok(response);
    }

    [HttpPost("create")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromForm] CreateRequestCommand command)
    {
        command.CustomerId = CurrentCustomerId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("send-message")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageCommand command)
    {
        command.CustomerId = CurrentCustomerId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("close")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Close([FromBody] CloseRequestCommand command)
    {
        // Ensure we use the correct customer ID from the token
        command.CustomerId = CurrentCustomerId;
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("{requestPublicId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(Guid requestPublicId)
    {
        var response = await _mediator.Send(new MarkMessagesReadCommand { RequestPublicId = requestPublicId, IsAgent = false });
        return Ok(response);
    }
}
