namespace HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;

public class PublicProjectDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
