using HelpCenter.Application.Features.Requests.Events;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Commands.CreateRequest;

public class CreateRequestCommandHandler : IRequestHandler<CreateRequestCommand, CreateRequestResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateRequestRules _rules;
    private readonly IFileService _fileService;
    private readonly IPublisher _publisher;
    private readonly ILogger<CreateRequestCommandHandler> _logger;

    public CreateRequestCommandHandler(
        IUnitOfWork unitOfWork,
        CreateRequestRules rules,
        IFileService fileService,
        IPublisher publisher,
        ILogger<CreateRequestCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _rules = rules;
        _fileService = fileService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<CreateRequestResponse> Handle(CreateRequestCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni talep oluşturma işlemi başlatıldı. Müşteri ID: {CustomerId}, Başlık: {Title}", request.CustomerId, request.Title);

        // 1. Customer validation
        var customer = (await _unitOfWork.Repository<Customer>()
            .FindAsync(x => x.Id == request.CustomerId, cancellationToken, c => c.Account)).FirstOrDefault();
        
        CreateRequestRules.CustomerShouldExist(customer);

        // 2. Title and request-subject rule validations
        var title = request.Title;
        if (request.RequestSubjectId.HasValue)
        {
            var subject = await _unitOfWork.Repository<RequestSubject>().GetAsync(request.RequestSubjectId.Value, cancellationToken);
            if (subject != null && string.IsNullOrEmpty(title))
            {
                title = subject.Name;
            }
            await _rules.LockSubjectIfUnlockedAsync(request.RequestSubjectId, cancellationToken);
        }

        // 3. Module access authorization and locking rules
        if (request.ModuleId.HasValue)
        {
            await _rules.ModuleShouldBeAllowedForCompanyAsync(customer!.CompanyId, request.ModuleId.Value, cancellationToken);
            await _rules.LockModuleIfUnlockedAsync(request.ModuleId.Value, cancellationToken);
        }

        // 4. Create the request entity
        var customerFullName = customer?.Account?.FullName ?? "Müşteri";
        var customerRequest = CustomerRequest.Create(
            request.CustomerId,
            request.ModuleId,
            request.RequestSubjectId,
            title,
            request.Priority,
            customerFullName
        );

        customerRequest.AddHistory(customer!.AccountId, RequestActionType.Created, null, "Talep Oluşturuldu");

        await _unitOfWork.Repository<CustomerRequest>().AddAsync(customerRequest, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _logger.LogInformation("Talep veritabanına kaydedildi. Ticket ID: {TicketId}", customerRequest.TicketId);

        // 5. Create the conversation and participant
        var conversation = new Conversation
        {
            Title = customerRequest.Title
        };
        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        customerRequest.ConversationId = conversation.Id;

        var participant = conversation.AddParticipant(request.CustomerId, null, ParticipantType.Customer);
        await _unitOfWork.Repository<ConversationParticipant>().AddAsync(participant, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        // 6. Create the message
        var initialMessage = new CustomerRequestMessage
        {
            ConversationId = conversation.Id,
            SenderParticipantId = participant.Id,
            MessageText = request.MessageText,
            Type = MessageType.Public
        };

        customerRequest.AddHistory(customer.AccountId, RequestActionType.MessageSent, null, "İlk Mesaj Gönderildi");

        await _unitOfWork.Repository<CustomerRequestMessage>().AddAsync(initialMessage, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        // 7. File attachments
        if (request.Files != null && request.Files.Count > 0)
        {
            _logger.LogInformation("Talebe {FileCount} adet dosya ekleniyor.", request.Files.Count);
            string folderPath = Path.Combine("MessageDocuments", request.CustomerId.ToString(), customerRequest.TicketId, initialMessage.Id.ToString());

            foreach (var file in request.Files)
            {
                string webPath = await _fileService.SaveFileAsync(file, folderPath);

                var document = new CustomerRequestMessageDocument
                {
                    CustomerRequestId = customerRequest.Id,
                    CustomerRequestMessageId = initialMessage.Id,
                    FileName = file.FileName,
                    Path = webPath
                };

                await _unitOfWork.Repository<CustomerRequestMessageDocument>().AddAsync(document, cancellationToken);
            }

            customerRequest.AddHistory(customer.AccountId, RequestActionType.DocumentAdded, null, $"{request.Files.Count} dosya eklendi");
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        _logger.LogInformation("Talep başarıyla oluşturuldu. Ticket ID: {TicketId}", customerRequest.TicketId);

        // 8. Publish domain events
        foreach (var domainEvent in customerRequest.DomainEvents)
        {
            if (domainEvent is RequestCreatedEvent evt)
            {
                _ = _publisher.Publish(new RequestCreatedNotification(evt), CancellationToken.None);
            }
        }
        customerRequest.ClearDomainEvents();

        return new CreateRequestResponse
        {
            TicketId = customerRequest.TicketId,
            PublicId = customerRequest.PublicId
        };
    }
}
