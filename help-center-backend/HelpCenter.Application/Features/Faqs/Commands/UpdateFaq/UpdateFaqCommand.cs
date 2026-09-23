using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int? ModuleId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
