using HelpCenter.Application.Features.Organization.Commands;
using HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;
using HelpCenter.Application.Features.Organization.Queries;
using HelpCenter.Application.Features.Organization.Queries.GetOrganizationInfo;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using HelpCenter.WebApi.Binders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/organization")]
[ApiController]
[Authorize]
public class AdminOrganizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileService _fileService;

    public AdminOrganizationController(IMediator mediator, IFileService fileService)
    {
        _mediator = mediator;
        _fileService = fileService;
    }

    [HttpGet]
    [HasPermission(AppResources.OrganizationSettings, PermissionActions.Read)]
    public async Task<IActionResult> GetOrganizationInfo()
    {
        var response = await _mediator.Send(new GetOrganizationInfoQuery());
        if (response == null) return NotFound(new { message = "Kurum bilgisi bulunamadı." });
        return Ok(response);
    }

    [HttpPut]
    [HasPermission(AppResources.OrganizationSettings, PermissionActions.Update)]
    [EnableRateLimiting("upload")]
    public async Task<IActionResult> UpdateOrganizationInfo([FromForm] UpdateOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrganizationName))
            return BadRequest(new { message = "Kurum adı zorunludur." });

        string? logoUrl = null;

        if (request.Logo != null && request.Logo.Length > 0)
        {
            _fileService.EnsureDirectoryExists("Organization");
            logoUrl = await _fileService.SaveFileAsync(request.Logo.ToFileUpload(), "Organization");
        }

        var command = new UpdateOrganizationInfoCommand
        {
            Id = request.Id,
            OrganizationName = request.OrganizationName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Website = request.Website,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            FooterText = request.FooterText,
            LogoUrl = logoUrl,
            RowVersion = request.RowVersion
        };

        bool result;
        try
        {
            result = await _mediator.Send(command);
        }
        catch
        {
            // The DB update (RowVersion conflict, DB error, etc.) failed — clean up the NEW logo
            // file just saved to disk. The old logo was never touched and the DB
            // still points to it, so it must not be deleted.
            if (logoUrl != null)
                _fileService.DeleteFile(logoUrl);

            throw;
        }

        if (!result)
        {
            if (logoUrl != null)
                _fileService.DeleteFile(logoUrl);

            return BadRequest(new { message = "Güncelleme başarısız." });
        }

        // The old logo file is deleted in the handler, AFTER SaveAsync SUCCEEDS
        // (see UpdateOrganizationInfoCommandHandler) — no need to query and delete it again here.
        return Ok(new { message = "Kurum bilgileri başarıyla güncellendi." });
    }

    public class UpdateOrganizationRequest
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; }
        public string? TaxOffice { get; set; }
        public string? FooterText { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public IFormFile? Logo { get; set; }
    }
}
