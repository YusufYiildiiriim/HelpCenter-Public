using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Requests.Commands.SendMessage;

public class SendMessageCommand : IRequest<bool>
{
    public Guid RequestPublicId { get; set; }
    public int CustomerId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public List<FileUpload>? Files { get; set; }
}
