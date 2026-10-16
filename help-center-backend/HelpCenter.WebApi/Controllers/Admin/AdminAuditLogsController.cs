using HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpCenter.WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize]
public class AdminAuditLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(AppResources.AuditLogs, PermissionActions.Read)]
    public async Task<IActionResult> Get([FromQuery] GetAuditLogsQuery query, CancellationToken ct) =>
        Ok(await mediator.Send(query, ct));
}
