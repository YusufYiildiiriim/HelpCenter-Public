using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerMessages;

public sealed record CustomerMessagesTicketSummary(
    int CustomerId,
    int? ConversationId,
    string? CustomerFirstName,
    string? CustomerLastName,
    bool CustomerHasAccount);

public class GetCustomerMessagesQueryHandler : IRequestHandler<GetCustomerMessagesQuery, List<CustomerMessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerMessagesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CustomerMessageDto>> Handle(GetCustomerMessagesQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultProjectedAsync(
            x => x.PublicId == request.RequestPublicId,
            x => new CustomerMessagesTicketSummary(
                x.CustomerId,
                x.ConversationId,
                x.Customer!.FirstName,
                x.Customer!.LastName,
                x.Customer!.Account != null),
            cancellationToken);

        GetCustomerMessagesRules.RequestShouldExist(ticket);
        GetCustomerMessagesRules.RequestShouldBelongToCustomer(ticket!, request.CustomerId);

        if (ticket!.ConversationId == null) return new List<CustomerMessageDto>();

        var customerName = ticket.CustomerHasAccount
            ? (ticket.CustomerFirstName + " " + ticket.CustomerLastName).Trim()
            : "Müşteri";

        var messages = await _unitOfWork.Repository<CustomerRequestMessage>().SelectAsync(
            x => x.ConversationId == ticket.ConversationId && x.Type == MessageType.Public,
            m => new CustomerMessageDto
            {
                Id = m.Id,
                MessageText = m.MessageText,
                IsAgent = m.SenderParticipant.Type != ParticipantType.Customer,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead,
                SenderName = m.SenderParticipant.Type == ParticipantType.Customer
                    ? (m.SenderParticipant.Customer != null ? (m.SenderParticipant.Customer.FirstName + " " + m.SenderParticipant.Customer.LastName).Trim() : customerName)
                    : (m.SenderParticipant.User != null ? (m.SenderParticipant.User.FirstName + " " + m.SenderParticipant.User.LastName).Trim() : "Temsilci"),
                Type = m.Type,
                Documents = m.Documents.Select(d => new CustomerMessageDocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    Path = d.Path
                }).ToList()
            },
            cancellationToken: cancellationToken);

        return messages.OrderBy(x => x.CreatedAt).ToList();
    }
}
