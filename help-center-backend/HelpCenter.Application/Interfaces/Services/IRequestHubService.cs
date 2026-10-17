using HelpCenter.Domain.Enums;

namespace HelpCenter.Application.Interfaces
{
    public interface IRequestHubService
    {
        Task NotifyNewMessage(string requestId, object messageData, MessageType type, CancellationToken cancellationToken);
        Task NotifyMessagesRead(string requestId, bool isAgent, CancellationToken cancellationToken);
        Task NotifyRequestStatusChanged(string requestId, Guid statusId, string status, CancellationToken cancellationToken);
    }
}
