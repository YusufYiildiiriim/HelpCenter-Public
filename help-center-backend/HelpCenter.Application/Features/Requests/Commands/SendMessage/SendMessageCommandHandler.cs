using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IRequestHubService _hubService;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        IRequestHubService hubService,
        ILogger<SendMessageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task<bool> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mesaj gönderme işlemi başlatıldı. Talep: {RequestPublicId}, Müşteri: {CustomerId}", request.RequestPublicId, request.CustomerId);

        var customerRequest = (await _unitOfWork.Repository<CustomerRequest>().FindAsync(
            x => x.PublicId == request.RequestPublicId,
            cancellationToken,
            x => x.Customer,
            x => x.Conversation!,
            x => x.Conversation!.Participants)).FirstOrDefault();

        SendMessageRules.RequestShouldExist(customerRequest);
        SendMessageRules.RequestShouldBelongToCustomer(customerRequest!, request.CustomerId);

        if (customerRequest != null)
        {
            SendMessageRules.RequestShouldNotBeCompleted(customerRequest);

            if (customerRequest.Conversation == null)
            {
                var conversation = new Conversation { Title = customerRequest.Title };
                await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
                customerRequest.ConversationId = conversation.Id;
                customerRequest.Conversation = conversation;
                await _unitOfWork.Repository<CustomerRequest>().UpdateAsync(customerRequest, cancellationToken);
            }

            var participant =
                customerRequest.Conversation.AddParticipant(request.CustomerId, null, ParticipantType.Customer);

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
                Type = MessageType.Public,
                IsRead = false
            };

            customerRequest.AddHistory(customerRequest.Customer.AccountId, RequestActionType.MessageSent, null,
                "Yeni Mesaj Gönderildi");

            await _unitOfWork.Repository<CustomerRequestMessage>().AddAsync(message, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            if (request.Files != null && request.Files.Count > 0)
            {
                _logger.LogInformation("Mesaja {FileCount} adet dosya ekleniyor.", request.Files.Count);
                string folderPath = Path.Combine("MessageDocuments", request.CustomerId.ToString(),
                    customerRequest.TicketId, message.Id.ToString());

                foreach (var file in request.Files)
                {
                    string webPath = await _fileService.SaveFileAsync(file, folderPath);

                    var document = new CustomerRequestMessageDocument
                    {
                        CustomerRequestId = customerRequest.Id,
                        CustomerRequestMessageId = message.Id,
                        FileName = file.FileName,
                        Path = webPath
                    };

                    await _unitOfWork.Repository<CustomerRequestMessageDocument>()
                        .AddAsync(document, cancellationToken);
                }

                customerRequest.AddHistory(customerRequest.Customer.AccountId, RequestActionType.DocumentAdded, null,
                    $"{request.Files.Count} dosya eklendi");
                await _unitOfWork.SaveAsync(cancellationToken);
            }

            customerRequest.SetStatus(RequestStatusConstants.WaitingForReply);
            await _unitOfWork.SaveAsync(cancellationToken);

            _logger.LogInformation("Mesaj başarıyla kaydedildi. Mesaj ID: {MessageId}", message.Id);

            var senderName = $"{customerRequest.Customer.FirstName} {customerRequest.Customer.LastName}".Trim();

            var documents = await _unitOfWork.Repository<CustomerRequestMessageDocument>()
                .FindAsync(d => d.CustomerRequestMessageId == message.Id, cancellationToken);

            await _hubService.NotifyNewMessage(customerRequest.PublicId.ToString(), new
            {
                Id = message.Id,
                MessageText = message.MessageText,
                CreatedAt = message.CreatedAt,
                IsAgent = false,
                SenderParticipantId = message.SenderParticipantId,
                SenderName = senderName,
                Documents = documents.Select(d => new { d.Id, d.FileName, d.Path }).ToList(),
                StatusId = customerRequest.StatusId,
                Type = message.Type
            }, message.Type, cancellationToken);

            _logger.LogInformation("Yeni mesaj bildirimi Hub üzerinden gönderildi. Talep: {TicketId}",
                customerRequest.TicketId);
        }

        return true;
    }
}
