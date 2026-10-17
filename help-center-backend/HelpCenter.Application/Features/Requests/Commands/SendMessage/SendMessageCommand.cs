using HelpCenter.Application.Common.Caching;
using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Requests.Commands.SendMessage;

public class SendMessageCommand : IRequest<bool>, IInvalidatesMessageCache
{
    public Guid RequestPublicId { get; set; }
    public Guid CustomerId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public List<FileUpload>? Files { get; set; }
}
