using HelpCenter.Application.Common.Caching;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public class MarkMessagesReadCommand : IRequest<bool>, IInvalidatesMessageCache
{
    public Guid RequestPublicId { get; set; }
    public bool IsAgent { get; set; }
    public bool OnlyAssignedToCurrentUser { get; set; }
}
