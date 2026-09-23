namespace HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;

public class ModuleExpertDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
