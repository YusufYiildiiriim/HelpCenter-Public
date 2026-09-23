using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.CreateFaq;

public class CreateFaqCommand : IRequest<bool>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int? ModuleId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
}
