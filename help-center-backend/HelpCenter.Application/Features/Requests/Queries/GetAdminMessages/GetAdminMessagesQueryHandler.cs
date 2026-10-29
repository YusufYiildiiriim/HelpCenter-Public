using HelpCenter.Application.Common.Caching;
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
    Guid? ConversationId,
    string? CustomerFirstName,
    string? CustomerLastName,
    bool CustomerHasAccount);

public sealed record AdminMessageCursor(DateTime CreatedAt, Guid Id);

public class GetAdminMessagesQueryHandler : IRequestHandler<GetAdminMessagesQuery, AdminMessagesResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ICacheService? _cache;

    public GetAdminMessagesQueryHandler(IUnitOfWork unitOfWork, IUserContext userContext, ICacheService? cache = null)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _cache = cache;
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

        var cursor = request.BeforeId.HasValue
            ? await _unitOfWork.Repository<CustomerRequestMessage>().FirstOrDefaultProjectedAsync(
                x => x.Id == request.BeforeId.Value && x.ConversationId == ticket.ConversationId,
                x => new AdminMessageCursor(x.CreatedAt, x.Id),
                cancellationToken)
            : null;

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
        Task<List<AdminMessageDto>> LoadMessages(CancellationToken token)
        {
            if (request.BeforeId.HasValue && cursor is null)
            {
                return Task.FromResult(new List<AdminMessageDto>());
            }

            return _unitOfWork.Repository<CustomerRequestMessage>().SelectTopDescendingAsync(
                x => x.ConversationId == ticket.ConversationId &&
                    (!request.BeforeId.HasValue || x.CreatedAt < cursor!.CreatedAt ||
                     (x.CreatedAt == cursor.CreatedAt && x.Id < cursor.Id)),
                x => x.CreatedAt,
                x => x.Id,
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
            token);
        }

        // Only the initial bounded window is cached; ownership/assignment is checked above on every read.
        var useCache = _cache != null && !request.BeforeId.HasValue && request.PageSize is 5 or 10;
        var key = $"messages:staff:{request.RequestPublicId}:{_userContext.UserId}:{request.OnlyAssignedToCurrentUser}:{request.PageSize}";
        var loaded = useCache
            ? await _cache!.GetOrCreateAsync(key, CacheCategory.Messages.Prefix(), LoadMessages, TimeSpan.FromSeconds(30), cancellationToken)
            : await LoadMessages(cancellationToken);
        var messages = loaded.ToList(); // Sorting/trimming must not mutate the cached collection.

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
