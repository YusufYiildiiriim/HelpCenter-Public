using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Domain.Events;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.ConsultExpert;

public class ConsultExpertCommandHandler : IRequestHandler<ConsultExpertCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestHubService _hubService;
    private readonly IEmailDispatcher _emailDispatcher;

    public ConsultExpertCommandHandler(
        IUnitOfWork unitOfWork,
        IRequestHubService hubService,
        IEmailDispatcher emailDispatcher)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
        _emailDispatcher = emailDispatcher;
    }

    public async Task<bool> Handle(ConsultExpertCommand request, CancellationToken cancellationToken)
    {
        var customerRequests = await _unitOfWork.Repository<CustomerRequest>().FindAsync(
            x => x.PublicId == request.RequestPublicId,
            cancellationToken,
            x => x.Customer!,
            x => x.Customer!.Company!,
            x => x.Conversation!,
            x => x.Conversation!.Participants);
        var customerRequest = customerRequests.FirstOrDefault();
        ConsultExpertRules.RequestShouldExist(customerRequest);

        var agent = (await _unitOfWork.Repository<User>().FindAsync(x => x.Id == request.AgentUserId, cancellationToken, x => x.Account)).FirstOrDefault();
        ConsultExpertRules.AgentShouldExist(agent);

        var expert = (await _unitOfWork.Repository<User>().FindAsync(x => x.Id == request.ExpertId, cancellationToken, x => x.Account)).FirstOrDefault();
        ConsultExpertRules.ExpertShouldExist(expert);

        customerRequest!.AssignExpert(
            request.ExpertId,
            agent!.AccountId,
            expert!.Account.Email,
            expert.Account.FullName,
            agent.Account.FullName,
            request.Note
        );

        await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        foreach (var domainEvent in customerRequest.DomainEvents)
        {
            if (domainEvent is ExpertAssignedEvent evt)
                _emailDispatcher.EnqueueExpertNotification(evt.ExpertEmail, evt.ExpertFullName, evt.AgentFullName, evt.RequestTitle);
        }
        customerRequest.ClearDomainEvents();

        var conversation = customerRequest.Conversation;

        if (conversation == null)
        {
            conversation = new Conversation { Title = customerRequest.Title };
            await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            customerRequest.ConversationId = conversation.Id;
            customerRequest.Conversation = conversation;
            await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
        }

        var participant = conversation.AddParticipant(null, request.AgentUserId, ParticipantType.Agent);

        if (participant.Id == 0)
        {
            await _unitOfWork.Repository<ConversationParticipant>().AddAsync(participant, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        var internalMessage = new CustomerRequestMessage
        {
            ConversationId = conversation.Id,
            SenderParticipantId = participant.Id,
            MessageText = $"[SİSTEM] {agent.Account.FullName} bu talebi teknik inceleme için {expert.Account.FullName} uzmanına yönlendirdi. Not: {request.Note}",
            Type = MessageType.InternalNote,
            IsRead = false
        };

        await _unitOfWork.Repository<CustomerRequestMessage>().AddAsync(internalMessage, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        await _hubService.NotifyNewMessage(customerRequest.PublicId.ToString(), new
        {
            Id = internalMessage.Id,
            MessageText = internalMessage.MessageText,
            CreatedAt = internalMessage.CreatedAt,
            IsAgent = true,
            SenderParticipantId = participant.Id,
            SenderName = agent.Account.FullName,
            StatusId = customerRequest.StatusId,
            Type = MessageType.InternalNote
        }, internalMessage.Type, cancellationToken);

        return true;
    }
}
