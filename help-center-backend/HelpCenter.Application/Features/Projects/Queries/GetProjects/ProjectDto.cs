namespace HelpCenter.Application.Features.Projects.Queries.GetProjects;

public class ProjectDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int CompanyCount { get; set; }
    public int UserCount { get; set; }
    public List<int> ModuleIds { get; set; } = new();
    public byte[] RowVersion { get; set; } = null!;
}
