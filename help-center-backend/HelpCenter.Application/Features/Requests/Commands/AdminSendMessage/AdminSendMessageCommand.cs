using HelpCenter.Domain.Enums;
using MediatR;
using HelpCenter.Application.Common.Models;

namespace HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;

public class AdminSendMessageCommand : IRequest<bool>
{
    public Guid RequestPublicId { get; set; }
    public int SenderUserId { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Public;
    public List<FileUpload>? Files { get; set; }
}
