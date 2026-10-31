using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Commands.CreateFaq;

public class CreateFaqCommand : IRequest<bool>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SourceLocale { get; set; }
    public List<LocalizedTextInput> Translations { get; set; } = new();
    public Guid? ProjectId { get; set; }
    public Guid? ModuleId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
}
