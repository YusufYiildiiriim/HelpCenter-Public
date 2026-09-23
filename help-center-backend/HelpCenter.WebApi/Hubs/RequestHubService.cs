using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace HelpCenter.WebApi.Hubs
{
    public class RequestHubService : IRequestHubService
    {
        private readonly IHubContext<RequestHub> _hubContext;

        public RequestHubService(IHubContext<RequestHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewMessage(string requestId, object messageData, MessageType type, CancellationToken cancellationToken)
        {
            string groupName = type == MessageType.InternalNote ? requestId + "_internal" : requestId;
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", messageData, cancellationToken);
        }

        public async Task NotifyMessagesRead(string requestId, int userId, CancellationToken cancellationToken)
        {
            await _hubContext.Clients.Group(requestId).SendAsync("MessagesRead", userId, cancellationToken);
        }
    }
}
