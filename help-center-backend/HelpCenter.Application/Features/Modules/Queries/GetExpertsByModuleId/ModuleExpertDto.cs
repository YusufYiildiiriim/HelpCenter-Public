namespace HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;

public class ModuleExpertDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
