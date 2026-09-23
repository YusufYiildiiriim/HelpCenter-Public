using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpCenter.WebApi.Hubs
{
    [Authorize]
    public class RequestHub : Hub
    {
        private readonly IUnitOfWork _uow;

        public RequestHub(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>Creates an RBAC scope from SignalR's authenticated connection user.</summary>
        private DataScopeService CreateScopeForCurrentConnection()
        {
            return new DataScopeService(_uow, new HubUserContext(Context.User));
        }

        public async Task JoinRequestGroup(string requestId)
        {
            var access = await AuthorizeRequestAccessAsync(requestId);
            await Groups.AddToGroupAsync(Context.ConnectionId, requestId);

            if (access.CanAccessInternal)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, requestId + "_internal");
            }
        }

        public async Task LeaveRequestGroup(string requestId)
        {
            var access = await AuthorizeRequestAccessAsync(requestId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, requestId);

            if (access.CanAccessInternal)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, requestId + "_internal");
            }
        }

        public async Task MarkAsRead(string requestId)
        {
            var access = await AuthorizeRequestAccessAsync(requestId);
            await Clients.Group(requestId).SendAsync("MessagesRead", access.UserId);
        }

        private async Task<RequestAccess> AuthorizeRequestAccessAsync(string requestId)
        {
            if (!Guid.TryParse(requestId, out var requestPublicId))
                throw new HubException("Geçersiz talep kimliği.");

            var userContext = new HubUserContext(Context.User);
            if (!userContext.IsAuthenticated || userContext.UserId <= 0)
                throw new HubException("Kimlik doğrulaması gerekli.");

            var scope = new DataScopeService(_uow, userContext);

            var customerRequest = await _uow.Repository<CustomerRequest>()
                .FirstOrDefaultAsync(x => x.PublicId == requestPublicId);
            if (customerRequest is null)
                throw new HubException("Talep bulunamadı.");

            if (userContext.IsCustomer)
            {
                if (customerRequest.CustomerId != userContext.UserId)
                    throw new HubException("Bu talebe erişim yetkiniz yok.");

                return new RequestAccess(false, userContext.UserId);
            }

            var canReadAllRequests = await scope.HasPermissionAsync(AppResources.Requests, PermissionActions.Read);
            var canReadAssignedRequest = await scope.HasPermissionAsync(AppResources.AssignedRequests, PermissionActions.Read)
                && (customerRequest.AssignedUserId == userContext.UserId || customerRequest.CurrentExpertId == userContext.UserId);

            if (!canReadAllRequests && !canReadAssignedRequest)
                throw new HubException("Bu talebe erişim yetkiniz yok.");

            return new RequestAccess(true, userContext.UserId);
        }

        private sealed record RequestAccess(bool CanAccessInternal, int UserId);
    }
}
