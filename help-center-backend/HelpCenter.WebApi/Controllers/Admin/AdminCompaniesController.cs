using HelpCenter.Application.Features.Companies.Commands;
using HelpCenter.Application.Features.Companies.Commands.CreateCompany;
using HelpCenter.Application.Features.Companies.Commands.DeleteCompany;
using HelpCenter.Application.Features.Companies.Commands.UpdateCompany;
using HelpCenter.Application.Features.Companies.Queries;
using HelpCenter.Application.Features.Companies.Queries.GetCompanies;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpCenter.WebApi.Controllers.Admin;

[Route("api/admin/companies")]
[ApiController]
[Authorize]
public class AdminCompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-all-companies")]
    [HasPermission(AppResources.Companies, PermissionActions.Read)]
    public async Task<IActionResult> GetAll([FromQuery] GetCompaniesQuery query)
    {
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("add-company")]
    [HasPermission(AppResources.Companies, PermissionActions.Create)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Add([FromBody] CreateCompanyCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPut("{publicId}")]
    [HasPermission(AppResources.Companies, PermissionActions.Update)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Update(Guid publicId, [FromBody] UpdateCompanyRequest request)
    {
        var command = new UpdateCompanyCommand
        {
            PublicId = publicId,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            Mail = request.Mail,
            ContactPersonName = request.ContactPersonName,
            ContactPersonSurname = request.ContactPersonSurname,
            ContactPersonEmail = request.ContactPersonEmail,
            ContactPersonPhone = request.ContactPersonPhone,
            IsDemoActive = request.IsDemoActive,
            BranchCount = request.BranchCount,
            ModuleIds = request.ModuleIds,
            PreviousSystem = request.PreviousSystem,
            RowVersion = request.RowVersion,
            Password = request.Password,
            ContactPersonUsername = request.ContactPersonUsername,
            ProjectId = request.ProjectId
        };

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    public record UpdateCompanyRequest(
        string Name,
        string Address,
        string Phone,
        string Mail,
        string ContactPersonName,
        string ContactPersonSurname,
        string ContactPersonEmail,
        string ContactPersonPhone,
        bool IsDemoActive,
        int BranchCount,
        List<Guid> ModuleIds,
        string PreviousSystem,
        Guid RowVersion,
        string? Password = null,
        string? ContactPersonUsername = null,
        Guid? ProjectId = null);

    [HttpDelete("{publicId}")]
    [HasPermission(AppResources.Companies, PermissionActions.Delete)]
    [EnableRateLimiting("admin-write")]
    public async Task<IActionResult> Delete(Guid publicId)
    {
        var response = await _mediator.Send(new DeleteCompanyCommand(publicId));
        return Ok(response);
    }
}
