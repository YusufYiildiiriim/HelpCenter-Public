using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public class MarkMessagesReadCommand : IRequest<bool>
{
    public Guid RequestPublicId { get; set; }
    public bool IsAgent { get; set; }
}
