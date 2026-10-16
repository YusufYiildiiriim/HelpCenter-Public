using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Constants;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public class MarkMessagesReadCommandHandler : IRequestHandler<MarkMessagesReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConversationReadService _conversationReadService;
    private readonly IRequestHubService _hubService;
    private readonly IUserContext _userContext;
    private readonly IDataScopeService _scope;

    public MarkMessagesReadCommandHandler(
        IUnitOfWork unitOfWork,
        IConversationReadService conversationReadService,
        IRequestHubService hubService,
        IUserContext userContext,
        IDataScopeService scope)
    {
        _unitOfWork = unitOfWork;
        _conversationReadService = conversationReadService;
        _hubService = hubService;
        _userContext = userContext;
        _scope = scope;
    }

    public async Task<bool> Handle(MarkMessagesReadCommand request, CancellationToken cancellationToken)
    {
        MarkMessagesReadRules.ReaderShouldBeAuthenticated(_userContext.IsAuthenticated, _userContext.UserId);
        MarkMessagesReadRules.ReaderShouldMatchPortal(_userContext.IsCustomer, request.IsAgent);
        var customerRequest = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultAsync(
            x => x.PublicId == request.RequestPublicId,
            cancellationToken: cancellationToken);

        MarkMessagesReadRules.RequestShouldExist(customerRequest);
        if (_userContext.IsCustomer)
            MarkMessagesReadRules.RequestShouldBelongToCustomer(customerRequest!, _userContext.UserId);
        else
        {
            var allowed = request.OnlyAssignedToCurrentUser
                ? await _scope.HasPermissionAsync(AppResources.AssignedRequests, PermissionActions.Read, cancellationToken)
                    && (customerRequest!.AssignedUserId == _userContext.UserId || customerRequest.CurrentExpertId == _userContext.UserId)
                : await _scope.HasPermissionAsync(AppResources.Requests, PermissionActions.Read, cancellationToken);
            MarkMessagesReadRules.StaffShouldHaveReadAccess(allowed);
        }

        // Existing requests with no conversation or no unread messages are successful no-ops.
        if (customerRequest!.ConversationId == null) return true;

        // The Include chain ("Messages.SenderParticipant"), tracking, and saving now live in
        // IConversationReadService (the Persistence layer) — Application says "mark messages as
        // read" without ever referencing EF Core.
        var markedCount = await _conversationReadService.MarkMessagesAsReadAsync(
            customerRequest.ConversationId.Value, request.IsAgent, cancellationToken);

        if (markedCount == 0) return true;

        await _hubService.NotifyMessagesRead(request.RequestPublicId.ToString(), request.IsAgent ? 1 : 0, cancellationToken);

        return true;
    }
}
