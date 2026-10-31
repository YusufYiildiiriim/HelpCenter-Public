using HelpCenter.Application.Common.Security;
using HelpCenter.Application.Features.Guides.Queries;
using HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;
using HelpCenter.Domain.Common;
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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var companyIdValue = User.FindFirst(AppClaimTypes.CompanyId)?.Value;
        if (!Guid.TryParse(companyIdValue, out var companyId)) return Forbid();

        var rawLocale = Request.Headers.AcceptLanguage.ToString().Split(',')[0].Split(';')[0];
        var locale = SupportedLocales.Normalize(rawLocale) ?? SupportedLocales.Default;
        var response = await _mediator.Send(new GetCustomerGuidesQuery(locale, companyId), cancellationToken);
        return Ok(response);
    }
}
