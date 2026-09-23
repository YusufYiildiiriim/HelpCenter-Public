using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.MarkMessagesRead;

public class MarkMessagesReadCommandHandler : IRequestHandler<MarkMessagesReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConversationReadService _conversationReadService;
    private readonly IRequestHubService _hubService;

    public MarkMessagesReadCommandHandler(
        IUnitOfWork unitOfWork,
        IConversationReadService conversationReadService,
        IRequestHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _conversationReadService = conversationReadService;
        _hubService = hubService;
    }

    public async Task<bool> Handle(MarkMessagesReadCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultAsync(
            x => x.PublicId == request.RequestPublicId,
            cancellationToken: cancellationToken);

        if (customerRequest?.ConversationId == null) return false;

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
