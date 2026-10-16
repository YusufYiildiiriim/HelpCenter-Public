using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Interfaces
{
    public interface IRequestHubService
    {
        Task NotifyNewMessage(string requestId, object messageData, MessageType type, CancellationToken cancellationToken);
        Task NotifyMessagesRead(string requestId, int userId, CancellationToken cancellationToken);
        Task NotifyRequestStatusChanged(string requestId, int statusId, string status, CancellationToken cancellationToken);
    }
}
