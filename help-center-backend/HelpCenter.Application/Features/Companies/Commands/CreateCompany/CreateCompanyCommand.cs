using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommand : IRequest<bool>
{
    public string Name { get; set; } = string.Empty;
    public string DefaultLocale { get; set; } = HelpCenter.Domain.Common.SupportedLocales.Default;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonSurname { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public bool IsDemoActive { get; set; }
    public int BranchCount { get; set; }
    public List<Guid> ModuleIds { get; set; } = new();
    public string PreviousSystem { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? ContactPersonUsername { get; set; }
    public Guid? ProjectId { get; set; }
}
