using System.Linq.Expressions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAuditLogsQuery, PaginatedResponse<AuditLogDto>>
{
    public async Task<PaginatedResponse<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<AuditLog, bool>> filter = log =>
            (string.IsNullOrEmpty(request.EventType) || log.EventType == request.EventType) &&
            (!request.ActorUserId.HasValue || log.ActorUserId == request.ActorUserId) &&
            (string.IsNullOrEmpty(request.TargetType) || log.TargetType == request.TargetType) &&
            (!request.From.HasValue || log.OccurredAt >= request.From) &&
            (!request.To.HasValue || log.OccurredAt <= request.To);
        var page = await unitOfWork.Repository<AuditLog>().GetPaginatedProjectedAsync(filter,
            log => new AuditLogDto(log.Id, log.EventType, log.ActorUserId, string.Empty, log.TargetType, log.TargetId, log.CorrelationId, log.OccurredAt),
            request.PageNumber, request.PageSize,
            orderByDescendingKey: log => log.OccurredAt,
            thenByDescendingKey: log => log.Id,
            cancellationToken: cancellationToken);

        var actorIds = page.Items
            .Where(log => log.ActorUserId.HasValue)
            .Select(log => log.ActorUserId!.Value)
            .Distinct()
            .ToArray();

        var actorNames = actorIds.Length == 0
            ? new Dictionary<Guid, string>()
            : (await unitOfWork.Repository<User>().SelectAsync(
                user => actorIds.Contains(user.Id),
                user => new { user.Id, DisplayName = user.Account.FirstName + " " + user.Account.LastName },
                cancellationToken: cancellationToken))
                .ToDictionary(actor => actor.Id, actor => actor.DisplayName.Trim());

        page.Items = page.Items.Select(log => log with
        {
            ActorDisplayName = !log.ActorUserId.HasValue
                ? "Sistem"
                : actorNames.GetValueOrDefault(log.ActorUserId.Value, "Silinmiş kullanıcı")
        }).ToList();

        return page;
    }
}
