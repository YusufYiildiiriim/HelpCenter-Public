namespace HelpCenter.Application.Features.Modules.Queries.GetPublicModules;

public class PublicModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
