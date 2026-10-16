using System.Linq.Expressions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAuditLogsQuery, PaginatedResponse<AuditLogDto>>
{
    public Task<PaginatedResponse<AuditLogDto>> Handle(GetAuditLogsQuery query, CancellationToken ct)
    {
        Expression<Func<AuditLog, bool>> filter = log =>
            (string.IsNullOrEmpty(query.EventType) || log.EventType == query.EventType) &&
            (!query.ActorUserId.HasValue || log.ActorUserId == query.ActorUserId) &&
            (string.IsNullOrEmpty(query.TargetType) || log.TargetType == query.TargetType) &&
            (!query.From.HasValue || log.OccurredAt >= query.From) &&
            (!query.To.HasValue || log.OccurredAt <= query.To);
        return unitOfWork.Repository<AuditLog>().GetPaginatedProjectedAsync(filter,
            log => new AuditLogDto(log.Id, log.EventType, log.ActorUserId, log.TargetType, log.TargetId, log.CorrelationId, log.OccurredAt),
            query.PageNumber, query.PageSize, orderByDescendingKey: log => log.Id, cancellationToken: ct);
    }
}
