using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Commands.AdminSendMessage;

public class AdminSendMessageCommandHandler : IRequestHandler<AdminSendMessageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestHubService _hubService;
    private readonly IFileService _fileService;

    public AdminSendMessageCommandHandler(
        IUnitOfWork unitOfWork,
        IRequestHubService hubService,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
        _fileService = fileService;
    }

    public async Task<bool> Handle(AdminSendMessageCommand request, CancellationToken cancellationToken)
    {
        var customerRequest = (await _unitOfWork.Repository<CustomerRequest>().FindAsync(
            x => x.PublicId == request.RequestPublicId,
            cancellationToken,
            x => x.Conversation!,
            x => x.Conversation!.Participants,
            x => x.Customer!,
            x => x.Customer!.Company!)).FirstOrDefault();

        AdminSendMessageRules.RequestShouldExist(customerRequest);

        var agent = (await _unitOfWork.Repository<User>().FindAsync(x => x.Id == request.SenderUserId, cancellationToken, x => x.Account)).FirstOrDefault();
        AdminSendMessageRules.AgentShouldExist(agent);

        AdminSendMessageRules.RequestShouldNotBeCompleted(customerRequest!);

        if (customerRequest!.Conversation == null)
        {
            var conversation = new Conversation { Title = customerRequest.Title };
            await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            customerRequest.ConversationId = conversation.Id;
            customerRequest.Conversation = conversation;
            await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
        }

        // If the sender is the expert currently assigned to the request, the participant is
        // marked as Expert; otherwise (a regular agent response) it stays as Agent.
        var senderParticipantType = customerRequest.CurrentExpertId == request.SenderUserId
            ? ParticipantType.Expert
            : ParticipantType.Agent;

        var participant = customerRequest.Conversation!.AddParticipant(null, request.SenderUserId, senderParticipantType);

        if (participant.Id == 0)
        {
            await _unitOfWork.Repository<ConversationParticipant>().AddAsync(participant, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        var message = new CustomerRequestMessage
        {
            ConversationId = customerRequest.Conversation.Id,
            SenderParticipantId = participant.Id,
            MessageText = request.MessageText,
            Type = request.Type,
            IsRead = false
        };

        customerRequest.AddHistory(agent!.AccountId, RequestActionType.MessageSent, null, "Yanıt Gönderildi", request.Type == MessageType.InternalNote ? "Özel Not" : null);

        await _unitOfWork.Repository<CustomerRequestMessage>().AddAsync(message, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        if (request.Files != null && request.Files.Count > 0)
        {
            var folderPath = $"MessageDocuments/{request.SenderUserId}/{customerRequest.TicketId}/{message.Id}";

            int fileIndex = 1;
            foreach (var file in request.Files)
            {
                // Attachments are named sequentially within the message (1.png, 2.pdf ...).
                var webPath = await _fileService.SaveFileAsAsync(file, folderPath, fileIndex.ToString(), cancellationToken);

                var document = new CustomerRequestMessageDocument
                {
                    CustomerRequestId = customerRequest.Id,
                    CustomerRequestMessageId = message.Id,
                    FileName = file.FileName,
                    Path = webPath
                };

                await _unitOfWork.Repository<CustomerRequestMessageDocument>().AddAsync(document, cancellationToken);
                fileIndex++;
            }

            customerRequest.AddHistory(agent.AccountId, RequestActionType.DocumentAdded, null, $"{request.Files.Count} dosya eklendi");
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        if (request.Type == MessageType.Public)
        {
            customerRequest.SetStatus(RequestStatusConstants.Replied, agent.AccountId, "Yanıtlandı");
            await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        var senderName = $"{agent!.FirstName} {agent.LastName}".Trim();

        var documents = await _unitOfWork.Repository<CustomerRequestMessageDocument>()
            .FindAsync(d => d.CustomerRequestMessageId == message.Id, cancellationToken);

        await _hubService.NotifyNewMessage(customerRequest.PublicId.ToString(), new
        {
            Id = message.Id,
            MessageText = message.MessageText,
            CreatedAt = message.CreatedAt,
            IsAgent = true,
            SenderParticipantId = message.SenderParticipantId,
            SenderName = senderName,
            Documents = documents.Select(d => new { d.Id, d.FileName, d.Path }).ToList(),
            StatusId = customerRequest.StatusId,
            Type = message.Type
        }, message.Type, cancellationToken);

        return true;
    }
}
