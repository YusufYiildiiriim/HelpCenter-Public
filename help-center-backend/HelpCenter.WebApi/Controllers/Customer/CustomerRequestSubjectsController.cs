using HelpCenter.Application.Features.RequestSubjects.Queries;
using HelpCenter.Application.Features.RequestSubjects.Queries.GetCustomerRequestSubjects;
using HelpCenter.Application.Common.Security;
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
        var raw = Request.Headers.AcceptLanguage.ToString().Split(',')[0].Split(';')[0];
        var locale = HelpCenter.Domain.Common.SupportedLocales.Normalize(raw) ?? HelpCenter.Domain.Common.SupportedLocales.Default;
        var customerId = Guid.TryParse(User.FindFirst(AppClaimTypes.UserId)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedCustomerId) ? parsedCustomerId : Guid.Empty;
        var companyId = Guid.TryParse(User.FindFirst(AppClaimTypes.CompanyId)?.Value, out var parsedCompanyId) ? parsedCompanyId : Guid.Empty;
        var response = await _mediator.Send(new GetCustomerRequestSubjectsQuery(locale, customerId, companyId));
        return Ok(response);
    }
}
