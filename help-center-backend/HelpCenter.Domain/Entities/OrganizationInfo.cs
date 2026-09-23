namespace HelpCenter.Domain.Entities;

public class OrganizationInfo : BaseEntity
{
    public string OrganizationName { get; set; } = "Help Center";
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? FooterText { get; set; }

    public static OrganizationInfo Create(
        string organizationName,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? website = null,
        string? taxNumber = null,
        string? taxOffice = null,
        string? footerText = null)
    {
        return new OrganizationInfo
        {
            OrganizationName = organizationName,
            Phone = phone,
            Email = email,
            Address = address,
            Website = website,
            TaxNumber = taxNumber,
            TaxOffice = taxOffice,
            FooterText = footerText,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string organizationName,
        string? phone,
        string? email,
        string? address,
        string? website,
        string? taxNumber,
        string? taxOffice,
        string? footerText)
    {
        OrganizationName = organizationName;
        Phone = phone;
        Email = email;
        Address = address;
        Website = website;
        TaxNumber = taxNumber;
        TaxOffice = taxOffice;
        FooterText = footerText;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
