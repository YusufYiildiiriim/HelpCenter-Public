using MediatR;

namespace HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;

public class UpdateOrganizationInfoCommand : IRequest<bool>
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
    public string? LogoUrl { get; set; }
    public byte[]? RowVersion { get; set; }
}
