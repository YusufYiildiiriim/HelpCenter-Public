namespace HelpCenter.Application.Features.Modules.Queries.GetModules;

public class ModuleDto
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
