using MediatR;

namespace HelpCenter.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommand : IRequest<bool>
{
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string ContactPersonSurname { get; set; } = string.Empty;
    public string ContactPersonEmail { get; set; } = string.Empty;
    public string ContactPersonPhone { get; set; } = string.Empty;
    public bool IsDemoActive { get; set; }
    public int BranchCount { get; set; }
    public List<int> ModuleIds { get; set; } = new();
    public string PreviousSystem { get; set; } = string.Empty;
    public byte[]? RowVersion { get; set; }
    public string? Password { get; set; }
    public string? ContactPersonUsername { get; set; }
    public int? ProjectId { get; set; }
}
