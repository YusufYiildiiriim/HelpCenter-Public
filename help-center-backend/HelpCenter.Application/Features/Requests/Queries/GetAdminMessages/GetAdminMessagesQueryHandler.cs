using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminMessages;

/// <summary>
/// An intermediate summary that directly projects the fields needed, instead of using
/// `FirstOrDefaultWithIncludesAsync` to load full entities and mapping them to a DTO. Requires
/// no string-path Include (`"Customer.Account"`) — EF Core infers the needed join from the
/// projection expression itself; the never-used `Customer.Company` join (present in the old
/// code but never read) is also gone.
/// </summary>
public sealed record AdminMessagesTicketSummary(
    int? ConversationId,
    string? CustomerFirstName,
    string? CustomerLastName,
    bool CustomerHasAccount);

public class GetAdminMessagesQueryHandler : IRequestHandler<GetAdminMessagesQuery, AdminMessagesResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public GetAdminMessagesQueryHandler(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<AdminMessagesResponse> Handle(GetAdminMessagesQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Repository<CustomerRequest>().FirstOrDefaultProjectedAsync(
            x => x.PublicId == request.RequestPublicId &&
                (!request.OnlyAssignedToCurrentUser ||
                 x.AssignedUserId == _userContext.UserId ||
                 x.CurrentExpertId == _userContext.UserId),
            x => new AdminMessagesTicketSummary(
                x.ConversationId,
                x.Customer!.FirstName,
                x.Customer!.LastName,
                x.Customer!.Account != null),
            cancellationToken);

        GetAdminMessagesRules.RequestShouldExist(ticket);

        if (ticket!.ConversationId == null) return new AdminMessagesResponse
        {
            Messages = new List<AdminMessageDto>(),
            HasMore = false,
            TotalCount = 0
        };

        var totalCount = await _unitOfWork.Repository<CustomerRequestMessage>().CountAsync(
            x => x.ConversationId == ticket.ConversationId,
            cancellationToken: cancellationToken);

        var customerName = ticket.CustomerHasAccount
            ? (ticket.CustomerFirstName + " " + ticket.CustomerLastName).Trim()
            : "Silinmiş Müşteri";

        // Ordering and pagination (Take) are now applied at the DB level, BEFORE ToListAsync —
        // the old code fetched all matching messages into memory and applied Take there (see
        // Threads.md, "GetAdminMessagesQueryHandler — Cursor Pagination Correct, Take In Memory",
        // also fixed by this refactor).
        var messages = await _unitOfWork.Repository<CustomerRequestMessage>().SelectTopDescendingAsync(
            x => x.ConversationId == ticket.ConversationId &&
                 (!request.BeforeId.HasValue || x.Id < request.BeforeId.Value),
            x => x.CreatedAt,
            request.PageSize + 1,
            m => new AdminMessageDto
            {
                Id = m.Id,
                MessageText = m.MessageText,
                IsAgent = m.SenderParticipant.Type != ParticipantType.Customer,
                CreatedAt = m.CreatedAt,
                SenderName = m.SenderParticipant.Type == ParticipantType.Customer
                    ? (m.SenderParticipant.Customer != null ? (m.SenderParticipant.Customer.FirstName + " " + m.SenderParticipant.Customer.LastName).Trim() : customerName)
                    : (m.SenderParticipant.User != null ? (m.SenderParticipant.User.FirstName + " " + m.SenderParticipant.User.LastName).Trim() : "Temsilci"),
                IsRead = m.IsRead,
                Type = m.Type,
                Documents = m.Documents.Select(d => new AdminMessageDocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    Path = d.Path
                }).ToList()
            },
            cancellationToken);

        var hasMore = messages.Count > request.PageSize;
        if (hasMore) messages = messages.Take(request.PageSize).ToList();

        messages.Reverse();

        return new AdminMessagesResponse
        {
            Messages = messages,
            HasMore = hasMore,
            TotalCount = totalCount
        };
    }
}
