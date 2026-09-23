namespace HelpCenter.Application.Features.Organization.Queries.GetPublicOrganizationInfo;

public class PublicOrganizationInfoDto
{
    public string OrganizationName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FooterText { get; set; }
}
