using HelpCenter.Application.Features.Requests.Commands;
using HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;
using HelpCenter.Application.Features.Requests.Commands.ConsultExpert;
using HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;
using HelpCenter.Application.Features.Requests.Queries;
using HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequestById;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Features.Requests.Queries.GetAssignedRequests;
using HelpCenter.Application.Features.Requests.Queries.GetRequestCompanies;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.RegularExpressions;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/requests")]
[ApiController]
[Authorize]
public class AdminRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRequestCsvExportService _csvExport;

    public AdminRequestsController(IMediator mediator, IRequestCsvExportService csvExport)
    {
        _mediator = mediator;
        _csvExport = csvExport;
    }

    [HttpGet("get-all")]
    [HasPermission(AppResources.Requests, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetAdminRequestsQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpGet("export.csv")]
    [HasPermission(AppResources.Requests, PermissionActions.Export)]
    public async Task ExportCsv([FromQuery] GetAdminRequestsQuery query, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/csv; charset=utf-8";
        Response.Headers.ContentDisposition = $"attachment; filename=HelpCenter_Talepler_{DateTime.UtcNow:yyyy-MM-dd}.csv";
        await Response.WriteAsync("\uFEFFTalep No,Başlık,Açıklama,Firma,Müşteri,Modül,Öncelik,Durum,Atanan Uzman,Mesaj Sayısı,Oluşturulma\r\n", cancellationToken);

        await foreach (var row in _csvExport.StreamAsync(query, cancellationToken))
        {
            var values = new[]
            {
                row.TicketId, row.Title, StripHtml(row.Description), row.CompanyName, row.CustomerName,
                row.ModuleName, row.PriorityName, row.Status, row.AssignedUserName,
                row.MessageCount.ToString(), row.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            };
            await Response.WriteAsync(string.Join(',', values.Select(CsvCell)) + "\r\n", cancellationToken);
        }
    }

    private static string StripHtml(string value) => Regex.Replace(value ?? string.Empty, "<[^>]*>", string.Empty);

    private static string CsvCell(string value)
    {
        var safe = value ?? string.Empty;
        if (safe.Length > 0 && safe[0] is '=' or '+' or '-' or '@') safe = "'" + safe;
        return '"' + safe.Replace("\"", "\"\"") + '"';
    }

    [HttpGet("get-by-id/{publicId}")]
    [HasPermission(AppResources.Requests, PermissionActions.Read)]
    public async Task<IActionResult> GetById(Guid publicId)
    {
        var response = await _mediator.Send(new GetAdminRequestByIdQuery(publicId));
        return Ok(response);
    }

    [HttpGet("assigned/{publicId}")]
    [HasPermission(AppResources.AssignedRequests, PermissionActions.Read)]
    public async Task<IActionResult> GetAssignedById(Guid publicId)
    {
        var response = await _mediator.Send(new GetAdminRequestByIdQuery(publicId, OnlyAssignedToCurrentUser: true));
        return Ok(response);
    }

    [HttpGet("{requestPublicId}/messages")]
    [HasPermission(AppResources.Requests, PermissionActions.Read)]
    public async Task<IActionResult> GetMessages(Guid requestPublicId, [FromQuery] int pageSize = 20, [FromQuery] int? beforeId = null)
    {
        var response = await _mediator.Send(new GetAdminMessagesQuery { RequestPublicId = requestPublicId, PageSize = pageSize, BeforeId = beforeId });
        return Ok(response);
    }

    [HttpGet("assigned/{requestPublicId}/messages")]
    [HasPermission(AppResources.AssignedRequests, PermissionActions.Read)]
    public async Task<IActionResult> GetAssignedMessages(Guid requestPublicId, [FromQuery] int pageSize = 20, [FromQuery] int? beforeId = null)
    {
        var response = await _mediator.Send(new GetAdminMessagesQuery
        {
            RequestPublicId = requestPublicId,
            PageSize = pageSize,
            BeforeId = beforeId,
            OnlyAssignedToCurrentUser = true
        });
        return Ok(response);
    }

    [HttpPost("consult-expert")]
    [HasPermission(AppResources.Requests, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> ConsultExpert([FromBody] ConsultExpertCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("send-message")]
    [HasPermission(AppResources.Requests, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> SendMessage([FromForm] AdminSendMessageCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpGet("companies")]
    [HasPermission(AppResources.Requests, PermissionActions.Read)]
    public async Task<IActionResult> GetCompanies()
    {
        var response = await _mediator.Send(new GetRequestCompaniesQuery());
        return Ok(response);
    }

    [HttpGet("assigned")]
    [HasPermission(AppResources.AssignedRequests, PermissionActions.Read)]
    public async Task<IActionResult> GetAssigned([FromQuery] GetAssignedRequestsQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("{requestPublicId}/mark-read")]
    [HasPermission(AppResources.Requests, PermissionActions.Read)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> MarkAsRead(Guid requestPublicId)
    {
        var response = await _mediator.Send(new MarkMessagesReadCommand { RequestPublicId = requestPublicId, IsAgent = true });
        return Ok(response);
    }
}
