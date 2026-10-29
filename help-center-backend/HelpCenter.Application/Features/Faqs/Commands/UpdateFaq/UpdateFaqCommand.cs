using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.UpdateFaq;

public class UpdateFaqCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public Guid? ModuleId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public Guid RowVersion { get; set; }
}
